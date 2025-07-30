using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.User
{
    public class ResetPasswordRequestDTO
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Token deve ter exatamente 6 dígitos")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Token deve conter apenas números")]
        public string Token { get; set; } = null!;
        
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Senha deve conter ao menos: 1 letra minúscula, 1 maiúscula, 1 número e 1 caractere especial")]
        public string NewPassword { get; set; } = null!;
        
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NewPassword", ErrorMessage = "Senhas não coincidem")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
