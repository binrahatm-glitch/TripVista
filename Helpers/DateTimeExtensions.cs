namespace TripVista.Helpers
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo PakistanTimeZone = ResolvePakistanTimeZone();

        private static TimeZoneInfo ResolvePakistanTimeZone()
        {
            try
            {
                // Windows ID
                return TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // IANA ID (Linux/macOS)
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
            }
        }

        public static DateTime ToPakistanTime(this DateTime dateTime)
        {
            var utc = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, PakistanTimeZone);
        }

        public static DateTime? ToPakistanTime(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToPakistanTime() : null;
        }

        public static DateTime PakistanNow() => DateTime.UtcNow.ToPakistanTime();
    }
}
