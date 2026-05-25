namespace iM3Helpdesk.API.Services;

public class BirthdayPostOptions
{
  public bool Enabled { get; set; } = true;

  // Runs daily at this time in IST.
  public int RunHourIst { get; set; } = 0;
  public int RunMinuteIst { get; set; } = 10;
}
