// Application/Common/Exceptions/ExceptionMessages.cs
namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Exceptions;

public static class ExceptionMessages
{
    public static class NotFound
    {
        public const string Ar = "لم يتم العثور على العنصر المطلوب.";
        public const string En = "The requested item was not found.";
    }

    public static class Unauthorized
    {
        public const string Ar = "ليس لديك صلاحية للقيام بهذا الإجراء.";
        public const string En = "You are not authorized to perform this action.";
    }

    public static class BadRequest
    {
        public const string Ar = "الطلب غير صحيح. يرجى مراجعة البيانات المدخلة.";
        public const string En = "Invalid request. Please check the submitted data.";
    }

    public static class ServerError
    {
        public const string Ar = "حدث خطأ داخلي في الخادم. يرجى المحاولة لاحقاً.";
        public const string En = "An internal server error occurred. Please try again later.";
    }

    public static class Forbidden
    {
        public const string Ar = "ليس لديك صلاحية الوصول إلى هذا المورد.";
        public const string En = "You do not have permission to access this resource.";
    }

    public static class Business
    {
        public const string Ar = "لا يمكن إتمام هذه العملية.";
        public const string En = "This operation cannot be completed.";
    }
}