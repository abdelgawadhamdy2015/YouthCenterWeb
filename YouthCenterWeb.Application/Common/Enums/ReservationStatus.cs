public enum ReservationStatus
{
    Pending = 0,
    AwaitingPayment = 1,   // NEW — slot is locked, payment in progress
    Confirmed = 2,
    Cancelled = 3,
    Completed = 4
}