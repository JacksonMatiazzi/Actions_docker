namespace MeuCrud.Models;

public record ClienteInput(
    string? nome,
    string? sobrenome,
    string? email,
    string? telefone,
    string? cpf,
    DateTime? data_nascimento,
    string? logradouro,
    string? numero,
    string? complemento,
    string? bairro,
    string? cidade,
    string? uf,
    string? cep,
    bool? is_active
);
