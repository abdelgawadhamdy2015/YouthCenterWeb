namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    /// <summary>
    /// Single source of truth for time-slot conflicts on a YouthCenterActivity.
    /// Checks BOTH Reservations and CupMatches so:
    ///   - a reservation cannot be made over a scheduled match, and
    ///   - a match cannot be scheduled over a confirmed/locked reservation,
    ///   - a match cannot be scheduled over another match on the same activity.
    /// </summary>
    public interface IAvailabilityRepo
    {
        /// <param name="excludeReservationId">
        /// Pass the reservation's own id when re-checking during an update,
        /// so it doesn't conflict with itself.
        /// </param>
        /// <param name="excludeMatchId">
        /// Pass the match's own id when rescheduling, so it doesn't conflict
        /// with itself.
        /// </param>
        Task<bool> HasConflictAsync(
            int youthCenterActivityId,
            DateTime date,
            TimeOnly start,
            TimeOnly end,
            int? excludeReservationId = null,
            int? excludeMatchId = null);
    }
}