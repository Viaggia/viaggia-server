namespace viaggia_server.DTOs.User
{
    public class RegisterRequest
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Telefone { get; set; }
        public string RoleNome { get; set; } // Ex: "CLIENTE", "ADMIN", etc.
    }
}