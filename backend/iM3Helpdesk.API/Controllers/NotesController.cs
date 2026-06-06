using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenant;

    public NotesController(ApplicationDbContext context, ICurrentTenantService tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(claim, out var id);
        return id;
    }

    private (Guid userId, Guid? orgId) GetCtx() => (GetUserId(), _tenant.OrganizationId);

    // ── Notebooks ────────────────────────────────────────────────────────────

    [HttpGet("books")]
    public async Task<IActionResult> GetBooks()
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var books = await _context.NoteBooks
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.OrganizationId == orgId.Value)
            .OrderBy(b => b.DisplayOrder).ThenBy(b => b.CreatedAt)
            .Select(b => new {
                b.Id, b.Name, b.Color, b.DisplayOrder, b.CreatedAt, b.UpdatedAt,
                sectionCount = b.Sections.Count()
            })
            .ToListAsync();

        return Ok(books);
    }

    [HttpPost("books")]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Name required" });

        var maxOrder = await _context.NoteBooks
            .Where(b => b.UserId == userId && b.OrganizationId == orgId.Value)
            .MaxAsync(b => (int?)b.DisplayOrder) ?? -1;

        var book = new NoteBook
        {
            UserId = userId,
            OrganizationId = orgId.Value,
            Name = dto.Name.Trim(),
            Color = dto.Color,
            DisplayOrder = maxOrder + 1
        };
        _context.NoteBooks.Add(book);
        await _context.SaveChangesAsync();
        return Ok(new { book.Id, book.Name, book.Color, book.DisplayOrder, book.CreatedAt, book.UpdatedAt, sectionCount = 0 });
    }

    [HttpPut("books/{id:guid}")]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookDto dto)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var book = await _context.NoteBooks.FirstOrDefaultAsync(b =>
            b.Id == id && b.UserId == userId && b.OrganizationId == orgId.Value);
        if (book == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Name)) book.Name = dto.Name.Trim();
        book.Color = dto.Color ?? book.Color;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { book.Id, book.Name, book.Color, book.DisplayOrder, book.CreatedAt, book.UpdatedAt });
    }

    [HttpDelete("books/{id:guid}")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var book = await _context.NoteBooks.FirstOrDefaultAsync(b =>
            b.Id == id && b.UserId == userId && b.OrganizationId == orgId.Value);
        if (book == null) return NotFound();

        _context.NoteBooks.Remove(book);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Sections ─────────────────────────────────────────────────────────────

    [HttpGet("books/{bookId:guid}/sections")]
    public async Task<IActionResult> GetSections(Guid bookId)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var sections = await _context.NoteSections
            .AsNoTracking()
            .Where(s => s.NoteBookId == bookId && s.UserId == userId && s.OrganizationId == orgId.Value)
            .OrderBy(s => s.DisplayOrder).ThenBy(s => s.CreatedAt)
            .Select(s => new {
                s.Id, s.NoteBookId, s.Name, s.Color, s.DisplayOrder, s.CreatedAt, s.UpdatedAt,
                pages = s.Pages
                    .OrderBy(p => p.DisplayOrder)
                    .ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                    .Select(p => new { p.Id, p.NoteSectionId, p.Title, p.DisplayOrder, p.CreatedAt, p.UpdatedAt })
                    .ToList()
            })
            .ToListAsync();

        return Ok(sections);
    }

    [HttpPost("sections")]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { message = "Name required" });

        var bookExists = await _context.NoteBooks.AnyAsync(b =>
            b.Id == dto.NoteBookId && b.UserId == userId && b.OrganizationId == orgId.Value);
        if (!bookExists) return NotFound(new { message = "Notebook not found" });

        var maxOrder = await _context.NoteSections
            .Where(s => s.NoteBookId == dto.NoteBookId && s.UserId == userId)
            .MaxAsync(s => (int?)s.DisplayOrder) ?? -1;

        var section = new NoteSection
        {
            UserId = userId,
            OrganizationId = orgId.Value,
            NoteBookId = dto.NoteBookId,
            Name = dto.Name.Trim(),
            DisplayOrder = maxOrder + 1
        };
        _context.NoteSections.Add(section);
        await _context.SaveChangesAsync();
        return Ok(new { section.Id, section.NoteBookId, section.Name, section.Color, section.DisplayOrder, section.CreatedAt, section.UpdatedAt, pages = new List<object>() });
    }

    [HttpPut("sections/{id:guid}")]
    public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateSectionDto dto)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var section = await _context.NoteSections.FirstOrDefaultAsync(s =>
            s.Id == id && s.UserId == userId && s.OrganizationId == orgId.Value);
        if (section == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Name)) section.Name = dto.Name.Trim();
        section.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { section.Id, section.Name, section.UpdatedAt });
    }

    [HttpDelete("sections/{id:guid}")]
    public async Task<IActionResult> DeleteSection(Guid id)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var section = await _context.NoteSections.FirstOrDefaultAsync(s =>
            s.Id == id && s.UserId == userId && s.OrganizationId == orgId.Value);
        if (section == null) return NotFound();

        _context.NoteSections.Remove(section);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Pages ─────────────────────────────────────────────────────────────────

    [HttpGet("pages/{id:guid}")]
    public async Task<IActionResult> GetPage(Guid id)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var page = await _context.NotePages
            .AsNoTracking()
            .Where(p => p.Id == id && p.UserId == userId && p.OrganizationId == orgId.Value)
            .Select(p => new { p.Id, p.NoteSectionId, p.Title, p.Content, p.DisplayOrder, p.CreatedAt, p.UpdatedAt })
            .FirstOrDefaultAsync();

        if (page == null) return NotFound();
        return Ok(page);
    }

    [HttpPost("pages")]
    public async Task<IActionResult> CreatePage([FromBody] CreatePageDto dto)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var sectionExists = await _context.NoteSections.AnyAsync(s =>
            s.Id == dto.NoteSectionId && s.UserId == userId && s.OrganizationId == orgId.Value);
        if (!sectionExists) return NotFound(new { message = "Section not found" });

        var maxOrder = await _context.NotePages
            .Where(p => p.NoteSectionId == dto.NoteSectionId && p.UserId == userId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? -1;

        var page = new NotePage
        {
            UserId = userId,
            OrganizationId = orgId.Value,
            NoteSectionId = dto.NoteSectionId,
            Title = string.IsNullOrWhiteSpace(dto.Title) ? "Untitled Page" : dto.Title.Trim(),
            Content = string.Empty,
            DisplayOrder = maxOrder + 1
        };
        _context.NotePages.Add(page);
        await _context.SaveChangesAsync();
        return Ok(new { page.Id, page.NoteSectionId, page.Title, page.Content, page.DisplayOrder, page.CreatedAt, page.UpdatedAt });
    }

    [HttpPut("pages/{id:guid}")]
    public async Task<IActionResult> UpdatePage(Guid id, [FromBody] UpdatePageDto dto)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var page = await _context.NotePages.FirstOrDefaultAsync(p =>
            p.Id == id && p.UserId == userId && p.OrganizationId == orgId.Value);
        if (page == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Title))
            page.Title = dto.Title.Trim();
        page.Content = dto.Content ?? string.Empty;
        page.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { page.Id, page.Title, page.UpdatedAt });
    }

    [HttpDelete("pages/{id:guid}")]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        var (userId, orgId) = GetCtx();
        if (userId == Guid.Empty || !orgId.HasValue) return Unauthorized();

        var page = await _context.NotePages.FirstOrDefaultAsync(p =>
            p.Id == id && p.UserId == userId && p.OrganizationId == orgId.Value);
        if (page == null) return NotFound();

        _context.NotePages.Remove(page);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record CreateBookDto(string Name, string? Color);
public record UpdateBookDto(string? Name, string? Color);
public record CreateSectionDto(Guid NoteBookId, string Name);
public record UpdateSectionDto(string Name);
public record CreatePageDto(Guid NoteSectionId, string? Title);
public record UpdatePageDto(string? Title, string? Content);
