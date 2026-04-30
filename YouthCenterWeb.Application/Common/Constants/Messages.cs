namespace YouthCenterWeb.Application.Common.Constants
{
    public static class Messages
    {
        public static class Auth
        {
            public const string InvalidEmailEn = "Invalid email";
            public const string InvalidEmailAr = "البريد الإلكتروني غير صحيح";

            public const string InvalidPasswordEn = "Invalid password";
            public const string InvalidPasswordAr = "كلمة المرور غير صحيحة";

            public const string EmailExistsEn = "Email already exists";
            public const string EmailExistsAr = "البريد مستخدم بالفعل";

            public const string LoginSuccessEn = "Login successful";
            public const string LoginSuccessAr = "تم تسجيل الدخول بنجاح";
        }

        public static class Data
        {
            public const string NoDataEn = "No Data";
            public const string NoDataAr = "لا يوجد بيانات";

            public const string FoundEn = "Activities found";
            public const string FoundAr = "تم العثور على البيانات";

            public const string CreatedEn = "Created Successfully";
            public const string CreatedAr = "تم الإنشاء بنجاح";

            public const string CreationFailedEn = "Creation Failed";
            public const string CreationFailedAr = "فشل الإنشاء";

            public static string UpdatedAr => "تم التحديث بنجاح";
            public static string UpdatedEn => "Updated Successfully";

            public static string DeletedAr => "تم الحذف بنجاح";
            public static string DeletedEn => "Deleted Successfully";

            public static string DeletionFailedAr => "فشل الحذف";
            public static string DeletionFailedEn => "Deletion Failed";


        }

        public static class Validation
        {
            public static string RequiredFieldAr(string field) => $"حقل {field} مطلوب.";
            public static string RequiredFieldEn(string field) => $"{field} is required.";

            public static string EmailRequiredAr => "البريد الإلكتروني مطلوب.";
            public static string EmailRequiredEn => "Email is required.";
        }
    }
}