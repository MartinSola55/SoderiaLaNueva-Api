namespace SoderiaLaNueva_Api.Models.Constants
{
    public class Messages
    {
        public class Error
        {
            public static string Exception()
            {
                return "Ha ocurrido un error inesperado. Por favor, intenta de nuevo.";
            }
            public static string Unauthorized()
            {
                return "No tienes permisos para realizar esta operación.";
            }
            public static string EntityNotFound(string entityName)
            {
                return entityName + " no encontrado.";
            }
            public static string SaveEntity(string entityName)
            {
                return "Ha ocurrido un error al intentar guardar " + entityName + ". Por favor, intenta de nuevo.";
            }
            public static string FieldsRequired()
            {
                return "Debes ingresar todos los campos obligatorios.";
            }
            public static string FieldsRequired(string[] fields)
            {
                return "Debes ingresar todos los campos obligatorios: " + string.Join(", ", fields);
            }
            public static string FieldRequired(string fieldName)
            {
                return "El campo " + fieldName + " es requerido.";
            }
            public static string FieldGraterThanZero(string fieldName)
            {
                return "El campo " + fieldName + " debe ser mayor a cero.";
            }
            public static string FieldGraterOrEqualThanZero(string fieldName)
            {
                return "El campo " + fieldName + " debe ser mayor o igual a cero.";
            }
            public static string InvalidField(string fieldName)
            {
                return "El campo " + fieldName + " no es válido.";
            }
            public static string UniqueField(string fieldName,string entityName)
            {
                return "Ya existe " + entityName + " con " + fieldName;
            }
            public static string DuplicateEntity(string entity, bool femine = false)
            {
                return "Ya existe un" + (femine ? "a" : "") + " " + entity + " con los datos ingresados.";
            }
            public static string InvalidPassword()
            {
                return "La contraseña debe contener al menos 8 caracteres, una letra mayúscula, una letra minúscula, un número y un caracter especial.";
            }
            public static string InvalidPhone()
            {
                return "El teléfono ingresado no es válido.";
            }
            public static string InvalidEmail()
            {
                return "El email ingresado no es válido.";
            }
            public static string DuplicateEmail()
            {
                return "El email ingresado ya se encuentra registrado.";
            }
            public static string InvalidLogin()
            {
                return "Email y/o contraseña inválidos.";
            }
            public static string InvalidToken()
            {
                return "El token ingresado no es válido. Por favor, inicia sesión nuevamente.";
            }
            public static string TokenCreation()
            {
                return "Ha ocurrido un error al intentar crear el token. Por favor, intenta de nuevo.";
            }
            public static string BlockedUser()
            {
                return "El usuario con el que intentas acceder ha sido bloqueado.";
            }
            public static string UserWithoutRole()
            {
                return "El usuario con el que intentas acceder no posee un rol.";
            }
            public static string UserDoesntHavePermission()
            {
                return "Tu rol no te permite realizar esta acción.";
            }
        }
        public class CRUD
        {
            public static string EntityCreated(string entityName)
            {
                return entityName + " creado correctamente.";
            }
            public static string EntityUpdated(string entityName)
            {
                return entityName + " editado correctamente.";
            }
            public static string EntityDeleted(string entityName)
            {
                return entityName + " eliminado correctamente.";
            }
        }
    }
}
