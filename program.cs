using Dapper;         
using Npgsql;
using MeuCrud.Models; 

var builder = WebApplication.CreateBuilder(args);

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

 //Connection string do appsettings.json
var cs = builder.Configuration.GetConnectionString("AppLarroude")
         ?? throw new InvalidOperationException("Connection string 'AppLarroude' não encontrada.");

//Raiz
app.MapGet("/", () => "API OK");

 //Testa conexão com o banco
app.MapGet("/db", async () =>
{
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    return Results.Ok(new { ok = true, version = conn.PostgreSqlVersion.ToString() });
});

 //GET /clientes → Le usuarios do banco (read)
app.MapGet("/clientes", async () =>
{
    const string sql = @"SELECT * FROM ""CRUD"".clientes ORDER BY id;";
    await using var conn = new NpgsqlConnection(cs);
    var rows = await conn.QueryAsync(sql);   // retorna IEnumerable<dynamic>
    return Results.Ok(rows);
});

 //POST /clientes  → cria cliente (create)
app.MapPost("/clientes", async (ClienteInput body) =>
{
    const string sql = @"
        INSERT INTO ""CRUD"".clientes
            (nome, sobrenome, email, telefone, cpf, data_nascimento,
             logradouro, numero, complemento, bairro, cidade, uf, cep,
             is_active, created_at, updated_at)
        VALUES
            (@nome, @sobrenome, @email, @telefone, @cpf, @data_nascimento,
             @logradouro, @numero, @complemento, @bairro, @cidade, @uf, @cep,
             COALESCE(@is_active, TRUE), NOW(), NOW())
        RETURNING *;";

    await using var conn = new NpgsqlConnection(cs);

    try
    {
        var created = await conn.QuerySingleAsync(sql, body); 

        var id = (int)created.id;
        return Results.Created($"/clientes/{id}", created);
    }
    catch (PostgresException ex) when (ex.SqlState == "23505")
    {
        return Results.Conflict(new { error = "Registro já existe para campo único", constraint = ex.ConstraintName });
    }
    catch (PostgresException ex) when (ex.SqlState == "22001")
    {
        return Results.BadRequest(new { error = "Valor maior que o permitido por uma coluna VARCHAR", detail = ex.MessageText });
    }
});


//POST → Exclui pelo Id do usuario (Delete)
app.MapDelete("/clientes/{id:int}", async (int id) =>
{
    const string sql =@"delete from ""CRUD"".clientes where id =@id;";
    await using var conn = new NpgsqlConnection(cs);
    var affected = await conn.ExecuteAsync(sql, new { id });

    if (affected > 0)
    {
        return Results.Ok(new { deleted = true });
    }
    else
    {
        return Results.NotFound(new { error = " cliente existe? digitou certo? pode ser um erro generico também" });
    }
});


app.Run();
