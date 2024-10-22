//using Dapper;
//using Microsoft.Data.SqlClient; // Importação correta
//using System.Data;
//using System.Diagnostics; // Para medir o tempo
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using System;

//class Program
//{
//    // Modelo para representar a consulta que trará o boleto com nome do cliente e setor
//    public class BoletoDetalhado
//    {
//        public int Id { get; set; }
//        public string Nome_Cliente { get; set; }
//        public string Descricao_Setor { get; set; }
//        public DateTime Data_Criacao { get; set; }
//        public DateTime Data_Vencimento { get; set; }
//        public decimal Valor { get; set; }
//        public decimal Desconto { get; set; }
//        public string Codigo_Barras { get; set; }
//    }

//    // Repositório para realizar a consulta paginada com nome do cliente e setor
//    public class BoletoRepository
//    {
//        private readonly string _connectionString;

//        public BoletoRepository(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public async Task<IEnumerable<BoletoDetalhado>> GetBoletosPaginadosDetalhadosAsync(int pageNumber, int pageSize)
//        {
//            using (IDbConnection db = new SqlConnection(_connectionString))
//            {
//                var offset = (pageNumber - 1) * pageSize;

//                string query = @"
//                    SELECT b.id, c.nome_cliente, s.descricao_setor, 
//                           b.data_criacao, b.data_vencimento, b.valor, 
//                           b.desconto, b.codigo_barras
//                    FROM boleto b
//                    JOIN cliente c ON b.id_cliente = c.id
//                    JOIN setor s ON b.id_setor = s.id
//                    ORDER BY b.id
//                    OFFSET @Offset ROWS
//                    FETCH NEXT @PageSize ROWS ONLY";

//                return await db.QueryAsync<BoletoDetalhado>(query, new { Offset = offset, PageSize = pageSize });
//            }
//        }
//    }

//    static async Task Main(string[] args)
//    {
//        // String de conexão diretamente no código (substitua com suas credenciais se necessário)
//        string connectionString = "Data Source=BRUNO;Initial Catalog=financeiro;Integrated Security=True;TrustServerCertificate=True;";

//        var boletoRepository = new BoletoRepository(connectionString);

//        int pageNumber = 1; // Defina a página inicial (pode modificar conforme necessário)
//        int pageSize = 10;  // Quantidade de registros por página

//        // Medindo o tempo da consulta
//        Stopwatch stopwatch = new Stopwatch();
//        stopwatch.Start();

//        var boletos = await boletoRepository.GetBoletosPaginadosDetalhadosAsync(pageNumber, pageSize);

//        stopwatch.Stop();

//        // Exibe os resultados no console
//        foreach (var boleto in boletos)
//        {
//            Console.WriteLine($"Id: {boleto.Id}, Cliente: {boleto.Nome_Cliente}, Setor: {boleto.Descricao_Setor}, Valor: {boleto.Valor}, Vencimento: {boleto.Data_Vencimento}");
//        }

//        // Exibe o tempo de execução
//        Console.WriteLine($"Tempo de execução: {stopwatch.ElapsedMilliseconds} ms");

//        // Adiciona uma pausa para manter o console aberto
//        Console.WriteLine("Pressione qualquer tecla para sair...");
//        Console.ReadLine();
//    }
//}



using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

// Definição das entidades
public class Cliente
{
    public int Id { get; set; }
    public string Nome_Cliente { get; set; } 
    public string Cpf { get; set; } 
    public string Telefone { get; set; } 
}

public class Setor
{
    public int Id { get; set; }
    public string Descricao_Setor { get; set; } 
}

public class Boleto
{
    public int Id { get; set; }
    public DateTime Data_Criacao { get; set; } 
    public DateTime Data_Vencimento { get; set; } 
    public decimal Valor { get; set; }
    public decimal Desconto { get; set; }
    public string Codigo_Barras { get; set; }

    public int Id_Cliente { get; set; } 
    public Cliente Cliente { get; set; }

    public int Id_Setor { get; set; } 
    public Setor Setor { get; set; }
}

// Definição do DbContext
public class FinanceiroContext : DbContext
{
    public DbSet<Boleto> Boletos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Setor> Setores { get; set; }

    public FinanceiroContext(DbContextOptions<FinanceiroContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boleto>(entity =>
        {
            entity.ToTable("boleto");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Data_Criacao).HasColumnName("data_criacao"); 
            entity.Property(e => e.Data_Vencimento).HasColumnName("data_vencimento"); 
            entity.Property(e => e.Valor).HasColumnName("valor");
            entity.Property(e => e.Desconto).HasColumnName("desconto");
            entity.Property(e => e.Codigo_Barras).HasColumnName("codigo_barras"); 
            entity.Property(e => e.Id_Cliente).HasColumnName("id_cliente"); 
            entity.Property(e => e.Id_Setor).HasColumnName("id_setor"); 

            entity.HasOne(e => e.Cliente)
                  .WithMany()
                  .HasForeignKey(e => e.Id_Cliente);

            entity.HasOne(e => e.Setor)
                  .WithMany()
                  .HasForeignKey(e => e.Id_Setor);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("cliente");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome_Cliente).HasColumnName("nome_cliente"); 
            entity.Property(e => e.Cpf).HasColumnName("cpf");
            entity.Property(e => e.Telefone).HasColumnName("telefone"); 
        });

        modelBuilder.Entity<Setor>(entity =>
        {
            entity.ToTable("setor");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao_Setor).HasColumnName("descricao_setor"); 
        });
    }
}

// Classe para representar o boleto detalhado
public class BoletoDetalhado
{
    public int Id { get; set; }
    public string Nome_Cliente { get; set; }
    public string Descricao_Setor { get; set; }
    public DateTime Data_Criacao { get; set; }
    public DateTime Data_Vencimento { get; set; }
    public decimal Valor { get; set; }
    public decimal Desconto { get; set; }
    public string Codigo_Barras { get; set; }
}

// Repositório para consultar boletos
public class BoletoRepository
{
    private readonly FinanceiroContext _context;

    public BoletoRepository(FinanceiroContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BoletoDetalhado>> GetBoletosPaginadosDetalhadosAsync(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;

        var query = _context.Boletos
            .Include(b => b.Cliente)
            .Include(b => b.Setor)
            .OrderBy(b => b.Id)
            .Skip(offset)
            .Take(pageSize)
            .Select(b => new BoletoDetalhado
            {
                Id = b.Id,
                Nome_Cliente = b.Cliente.Nome_Cliente, 
                Descricao_Setor = b.Setor.Descricao_Setor, 
                Data_Criacao = b.Data_Criacao,
                Data_Vencimento = b.Data_Vencimento, 
                Valor = b.Valor,
                Desconto = b.Desconto,
                Codigo_Barras = b.Codigo_Barras 
            });

        return await query.ToListAsync();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Configurando o DbContext usando SQL Server
        var optionsBuilder = new DbContextOptionsBuilder<FinanceiroContext>();
        optionsBuilder.UseSqlServer("Data Source=BRUNO;Initial Catalog=financeiro;Integrated Security=True;TrustServerCertificate=True;");

        using (var context = new FinanceiroContext(optionsBuilder.Options))
        {
            var boletoRepository = new BoletoRepository(context);

            int pageNumber = 1; 
            int pageSize = 10;  

            // Medindo o tempo da consulta
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var boletos = await boletoRepository.GetBoletosPaginadosDetalhadosAsync(pageNumber, pageSize);

            stopwatch.Stop();

           
            foreach (var boleto in boletos)
            {
                Console.WriteLine($"Id: {boleto.Id}, Cliente: {boleto.Nome_Cliente}, Setor: {boleto.Descricao_Setor}, Valor: {boleto.Valor}, Vencimento: {boleto.Data_Vencimento}");
            }

           
            Console.WriteLine($"Tempo de execução: {stopwatch.ElapsedMilliseconds} ms");

            
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadLine();
        }
    }
}

