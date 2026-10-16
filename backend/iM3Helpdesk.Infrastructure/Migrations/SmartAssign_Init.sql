-- ============================================================================
-- Smart Assign Agent Database Migration Script
-- Tables, Indexes, and Stored Procedures for DeskMate AI Command Center
-- ============================================================================

-- 1. AI_SmartAssignConfigs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AISmartAssignConfigs')
BEGIN
    CREATE TABLE [dbo].[AISmartAssignConfigs] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
        [Mode] NVARCHAR(50) NOT NULL DEFAULT 'Suggest', -- 'Suggest' or 'Auto'
        [ConfidenceThreshold] FLOAT NOT NULL DEFAULT 0.75,
        [WeightMl] FLOAT NOT NULL DEFAULT 0.60,
        [WeightWorkload] FLOAT NOT NULL DEFAULT 0.20,
        [WeightAvailability] FLOAT NOT NULL DEFAULT 0.20,
        [AutoRetrainFrequencyDays] INT NOT NULL DEFAULT 7,
        [MinTrainingSamples] INT NOT NULL DEFAULT 50,
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_AISmartAssignConfigs_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_AISmartAssignConfigs_OrganizationId] ON [dbo].[AISmartAssignConfigs]([OrganizationId]);
END
GO

-- 2. AI_ModelVersions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AIModelVersions')
BEGIN
    CREATE TABLE [dbo].[AIModelVersions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
        [Version] NVARCHAR(100) NOT NULL,
        [Algorithm] NVARCHAR(100) NOT NULL DEFAULT 'SdcaMaximumEntropy',
        [Accuracy] FLOAT NOT NULL DEFAULT 0.0,
        [Top3Accuracy] FLOAT NOT NULL DEFAULT 0.0,
        [TrainingSamplesCount] INT NOT NULL DEFAULT 0,
        [ModelFilePath] NVARCHAR(500) NOT NULL DEFAULT '',
        [MetricsJson] NVARCHAR(MAX) NOT NULL DEFAULT '{}',
        [IsActive] BIT NOT NULL DEFAULT 1,
        [TrainedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_AIModelVersions_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AIModelVersions_Org_Active] ON [dbo].[AIModelVersions]([OrganizationId], [IsActive]);
    CREATE INDEX [IX_AIModelVersions_TrainedAt] ON [dbo].[AIModelVersions]([TrainedAt]);
END
GO

-- 3. AI_AgentSkillStats Table (Precomputed category performance metrics per agent)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AIAgentSkillStats')
BEGIN
    CREATE TABLE [dbo].[AIAgentSkillStats] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
        [AgentId] UNIQUEIDENTIFIER NOT NULL,
        [Category] NVARCHAR(200) NOT NULL,
        [TotalResolved] INT NOT NULL DEFAULT 0,
        [AvgResolutionTimeHours] FLOAT NOT NULL DEFAULT 0.0,
        [SlaComplianceRate] FLOAT NOT NULL DEFAULT 0.0,
        [ReopenRate] FLOAT NOT NULL DEFAULT 0.0,
        [LastResolvedAt] DATETIME2 NULL,
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_AIAgentSkillStats_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AIAgentSkillStats_Users] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_AIAgentSkillStats_Org_Agent_Cat] ON [dbo].[AIAgentSkillStats]([OrganizationId], [AgentId], [Category]);
    CREATE INDEX [IX_AIAgentSkillStats_AgentId] ON [dbo].[AIAgentSkillStats]([AgentId]);
END
GO

-- 4. AI_AssignmentLogs Table (Decision tracking & override feedback loop)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AIAssignmentLogs')
BEGIN
    CREATE TABLE [dbo].[AIAssignmentLogs] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [TicketId] UNIQUEIDENTIFIER NOT NULL,
        [SuggestedAgentId] UNIQUEIDENTIFIER NOT NULL,
        [FinalAgentId] UNIQUEIDENTIFIER NULL,
        [ConfidenceScore] FLOAT NOT NULL,
        [MlScore] FLOAT NOT NULL,
        [WorkloadScore] FLOAT NOT NULL,
        [AvailabilityScore] FLOAT NOT NULL,
        [ReasonJson] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        [Mode] NVARCHAR(50) NOT NULL DEFAULT 'Suggest',
        [WasAutoAssigned] BIT NOT NULL DEFAULT 0,
        [IsOverridden] BIT NOT NULL DEFAULT 0,
        [OverriddenByUserId] UNIQUEIDENTIFIER NULL,
        [OverriddenAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [FK_AIAssignmentLogs_Tickets] FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Tickets]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AIAssignmentLogs_SuggestedAgent] FOREIGN KEY ([SuggestedAgentId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AIAssignmentLogs_FinalAgent] FOREIGN KEY ([FinalAgentId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AIAssignmentLogs_OverriddenByUser] FOREIGN KEY ([OverriddenByUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AIAssignmentLogs_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_AIAssignmentLogs_TicketId] ON [dbo].[AIAssignmentLogs]([TicketId]);
    CREATE INDEX [IX_AIAssignmentLogs_Org_Created] ON [dbo].[AIAssignmentLogs]([OrganizationId], [CreatedAt]);
    CREATE INDEX [IX_AIAssignmentLogs_IsOverridden] ON [dbo].[AIAssignmentLogs]([IsOverridden]);
END
GO

-- ============================================================================
-- STORED PROCEDURE: sp_GetHistoricalTrainingDataForSmartAssign
-- Retrieves sanitized historical training set (resolved tickets with agent label)
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetHistoricalTrainingDataForSmartAssign]
    @OrganizationId UNIQUEIDENTIFIER,
    @MinDaysBack INT = 365,
    @MaxRecords INT = 15000
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@MinDaysBack, SYSUTCDATETIME());

    SELECT TOP (@MaxRecords)
        t.[Id] AS [TicketId],
        ISNULL(t.[Title], '') AS [Title],
        ISNULL(t.[Description], '') AS [Description],
        ISNULL(t.[Category], 'General') AS [Category],
        CAST(t.[Priority] AS NVARCHAR(20)) AS [Priority],
        ISNULL(t.[TicketType], 'Support') AS [TicketType],
        ISNULL(t.[Tags], '') AS [Tags],
        ISNULL(t.[FromName], '') AS [CustomerName],
        ISNULL(t.[FromEmail], '') AS [CustomerEmail],
        t.[AssignedToUserId] AS [AgentId],
        u.[FullName] AS [AgentName],
        t.[CreatedAt],
        t.[ResolvedAt],
        DATEDIFF(MINUTE, t.[CreatedAt], ISNULL(t.[ResolvedAt], t.[UpdatedAt])) AS [ResolutionMinutes],
        CASE WHEN t.[IsSlaBreached] = 1 THEN 0 ELSE 1 END AS [MetSla]
    FROM [dbo].[Tickets] t WITH (NOLOCK)
    INNER JOIN [dbo].[Users] u WITH (NOLOCK) ON t.[AssignedToUserId] = u.[Id]
    WHERE t.[OrganizationId] = @OrganizationId
      AND t.[IsDeleted] = 0
      AND t.[AssignedToUserId] IS NOT NULL
      AND (t.[Status] IN (2, 3, 5)) -- Resolved, Closed, ResolvedOnBeta
      AND t.[CreatedAt] >= @CutoffDate
      AND u.[Role] IN (1, 2) -- CompanyAdmin, Agent
    ORDER BY t.[ResolvedAt] DESC;
END
GO

-- ============================================================================
-- STORED PROCEDURE: sp_RefreshAgentSkillStats
-- Recomputes agent category mastery, resolution speed, and SLA compliance
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_RefreshAgentSkillStats]
    @OrganizationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ReopenedTickets AS (
        SELECT 
            t.[Id] AS [TicketId],
            CASE WHEN EXISTS (
                SELECT 1 FROM [dbo].[TicketComments] tc WITH (NOLOCK)
                WHERE tc.[TicketId] = t.[Id] 
                  AND tc.[CreatedAt] > t.[ResolvedAt]
            ) THEN 1.0 ELSE 0.0 END AS [IsReopened]
        FROM [dbo].[Tickets] t WITH (NOLOCK)
        WHERE t.[OrganizationId] = @OrganizationId
          AND t.[IsDeleted] = 0
          AND t.[AssignedToUserId] IS NOT NULL
          AND t.[Status] IN (2, 3, 5)
    ),
    ResolvedStats AS (
        SELECT 
            t.[OrganizationId],
            t.[AssignedToUserId] AS [AgentId],
            ISNULL(t.[Category], 'General') AS [Category],
            COUNT(1) AS [TotalResolved],
            AVG(CAST(DATEDIFF(MINUTE, t.[CreatedAt], ISNULL(t.[ResolvedAt], t.[UpdatedAt])) AS FLOAT)) / 60.0 AS [AvgResolutionTimeHours],
            AVG(CASE WHEN t.[IsSlaBreached] = 0 THEN 1.0 ELSE 0.0 END) * 100.0 AS [SlaComplianceRate],
            AVG(rt.[IsReopened]) * 100.0 AS [ReopenRate],
            MAX(t.[ResolvedAt]) AS [LastResolvedAt]
        FROM [dbo].[Tickets] t WITH (NOLOCK)
        INNER JOIN [dbo].[Users] u WITH (NOLOCK) ON t.[AssignedToUserId] = u.[Id]
        INNER JOIN ReopenedTickets rt ON t.[Id] = rt.[TicketId]
        WHERE t.[OrganizationId] = @OrganizationId
          AND t.[IsDeleted] = 0
          AND t.[AssignedToUserId] IS NOT NULL
          AND t.[Status] IN (2, 3, 5)
        GROUP BY t.[OrganizationId], t.[AssignedToUserId], t.[Category]
    )
    MERGE [dbo].[AIAgentSkillStats] AS target
    USING ResolvedStats AS source
    ON (target.[OrganizationId] = source.[OrganizationId] 
        AND target.[AgentId] = source.[AgentId] 
        AND target.[Category] = source.[Category])
    WHEN MATCHED THEN
        UPDATE SET 
            target.[TotalResolved] = source.[TotalResolved],
            target.[AvgResolutionTimeHours] = ROUND(source.[AvgResolutionTimeHours], 2),
            target.[SlaComplianceRate] = ROUND(source.[SlaComplianceRate], 2),
            target.[ReopenRate] = ROUND(source.[ReopenRate], 2),
            target.[LastResolvedAt] = source.[LastResolvedAt],
            target.[UpdatedAt] = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (
            [Id], [OrganizationId], [AgentId], [Category], 
            [TotalResolved], [AvgResolutionTimeHours], [SlaComplianceRate], 
            [ReopenRate], [LastResolvedAt], [UpdatedAt]
        )
        VALUES (
            NEWID(), source.[OrganizationId], source.[AgentId], source.[Category],
            source.[TotalResolved], ROUND(source.[AvgResolutionTimeHours], 2), ROUND(source.[SlaComplianceRate], 2),
            ROUND(source.[ReopenRate], 2), source.[LastResolvedAt], SYSUTCDATETIME()
        );
END
GO

-- ============================================================================
-- STORED PROCEDURE: sp_GetAgentWorkloadAndAvailability
-- Computes real-time workload (active tickets, high priority) + online state
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAgentWorkloadAndAvailability]
    @OrganizationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.[Id] AS [AgentId],
        u.[FullName] AS [AgentName],
        u.[Email] AS [AgentEmail],
        u.[PhotoUrl] AS [AgentPhotoUrl],
        CAST(u.[Role] AS NVARCHAR(20)) AS [Role],
        ISNULL(uos.[IsOnline], 0) AS [IsOnline],
        uos.[LastSeen] AS [LastSeen],
        COUNT(CASE WHEN t.[Status] IN (0, 1, 4) AND t.[IsDeleted] = 0 THEN 1 END) AS [ActiveTicketCount],
        COUNT(CASE WHEN t.[Status] IN (0, 1, 4) AND t.[IsDeleted] = 0 AND t.[Priority] IN (2, 3) THEN 1 END) AS [HighPriorityTicketCount],
        COUNT(CASE WHEN CAST(t.[CreatedAt] AS DATE) = CAST(SYSUTCDATETIME() AS DATE) AND t.[AssignedToUserId] = u.[Id] THEN 1 END) AS [AssignedTodayCount]
    FROM [dbo].[Users] u WITH (NOLOCK)
    LEFT JOIN [dbo].[UserOnlineStatus] uos WITH (NOLOCK) ON u.[Id] = uos.[UserId]
    LEFT JOIN [dbo].[Tickets] t WITH (NOLOCK) ON u.[Id] = t.[AssignedToUserId] AND t.[OrganizationId] = @OrganizationId AND t.[IsDeleted] = 0
    WHERE u.[OrganizationId] = @OrganizationId
      AND u.[Role] IN (1, 2) -- CompanyAdmin, Agent
    GROUP BY u.[Id], u.[FullName], u.[Email], u.[PhotoUrl], u.[Role], uos.[IsOnline], uos.[LastSeen]
    ORDER BY [ActiveTicketCount] ASC, [HighPriorityTicketCount] ASC;
END
GO

-- ============================================================================
-- STORED PROCEDURE: sp_GetSmartAssignStats
-- High performance summary for AI Command Center Top Cards and Metrics
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSmartAssignStats]
    @OrganizationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Unassigned Tickets
    DECLARE @UnassignedCount INT;
    SELECT @UnassignedCount = COUNT(1)
    FROM [dbo].[Tickets] WITH (NOLOCK)
    WHERE [OrganizationId] = @OrganizationId
      AND [IsDeleted] = 0
      AND [AssignedToUserId] IS NULL
      AND [Status] IN (0, 4); -- Open, Pending

    -- Auto-Assigned Today & Total
    DECLARE @AutoAssignedToday INT, @AutoAssignedTotal INT;
    SELECT 
        @AutoAssignedToday = COUNT(CASE WHEN CAST([CreatedAt] AS DATE) = CAST(SYSUTCDATETIME() AS DATE) AND [WasAutoAssigned] = 1 THEN 1 END),
        @AutoAssignedTotal = COUNT(CASE WHEN [WasAutoAssigned] = 1 THEN 1 END)
    FROM [dbo].[AIAssignmentLogs] WITH (NOLOCK)
    WHERE [OrganizationId] = @OrganizationId;

    -- Override Rate
    DECLARE @TotalDecisions INT, @OverrideCount INT, @OverrideRate FLOAT;
    SELECT 
        @TotalDecisions = COUNT(1),
        @OverrideCount = COUNT(CASE WHEN [IsOverridden] = 1 THEN 1 END)
    FROM [dbo].[AIAssignmentLogs] WITH (NOLOCK)
    WHERE [OrganizationId] = @OrganizationId;

    SET @OverrideRate = CASE WHEN @TotalDecisions > 0 THEN ROUND((CAST(@OverrideCount AS FLOAT) / @TotalDecisions) * 100.0, 1) ELSE 0.0 END;

    -- Active Model Version Info
    DECLARE @ModelVersion NVARCHAR(100), @ModelAccuracy FLOAT, @ModelTop3Accuracy FLOAT, @ModelTrainedAt DATETIME2, @ModelSamples INT;
    SELECT TOP 1 
        @ModelVersion = [Version],
        @ModelAccuracy = [Accuracy],
        @ModelTop3Accuracy = [Top3Accuracy],
        @ModelTrainedAt = [TrainedAt],
        @ModelSamples = [TrainingSamplesCount]
    FROM [dbo].[AIModelVersions] WITH (NOLOCK)
    WHERE [OrganizationId] = @OrganizationId AND [IsActive] = 1
    ORDER BY [TrainedAt] DESC;

    -- Current Config
    DECLARE @Mode NVARCHAR(50), @ConfidenceThreshold FLOAT, @WeightMl FLOAT, @WeightWorkload FLOAT, @WeightAvailability FLOAT;
    SELECT TOP 1
        @Mode = [Mode],
        @ConfidenceThreshold = [ConfidenceThreshold],
        @WeightMl = [WeightMl],
        @WeightWorkload = [WeightWorkload],
        @WeightAvailability = [WeightAvailability]
    FROM [dbo].[AISmartAssignConfigs] WITH (NOLOCK)
    WHERE [OrganizationId] = @OrganizationId;

    SELECT 
        @UnassignedCount AS [UnassignedTicketsCount],
        ISNULL(@AutoAssignedToday, 0) AS [AutoAssignedToday],
        ISNULL(@AutoAssignedTotal, 0) AS [AutoAssignedTotal],
        ISNULL(@ModelAccuracy, 0.0) AS [ModelAccuracy],
        ISNULL(@ModelTop3Accuracy, 0.0) AS [ModelTop3Accuracy],
        ISNULL(@OverrideRate, 0.0) AS [OverrideRate],
        ISNULL(@TotalDecisions, 0) AS [TotalDecisions],
        ISNULL(@OverrideCount, 0) AS [OverrideCount],
        ISNULL(@ModelVersion, 'None') AS [ActiveModelVersion],
        @ModelTrainedAt AS [LastTrainedAt],
        ISNULL(@ModelSamples, 0) AS [TrainingSamplesCount],
        ISNULL(@Mode, 'Suggest') AS [Mode],
        ISNULL(@ConfidenceThreshold, 0.75) AS [ConfidenceThreshold],
        ISNULL(@WeightMl, 0.60) AS [WeightMl],
        ISNULL(@WeightWorkload, 0.20) AS [WeightWorkload],
        ISNULL(@WeightAvailability, 0.20) AS [WeightAvailability];
END
GO
