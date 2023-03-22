# Домашняя работа № 2. Добавляем взаимодействие между клиентом и сервером

### 1. Создать эндпоинты в проекте WebApi

Для решения задачи добавил класс CustomerContext для работы с БД с помощью EntityFrameworkCore:
```cs
public class CustomerContext: DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerContext()
    {
            
    }

    public CustomerContext(DbContextOptions<CustomerContext> options): base(options) 
    {

    }
}
```

Добавил интерфейс репозитория и класс реализующий этот интерфейс:

```cs
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetAsync(long id);

    Task AddAsync(T entity);
}
```

```cs
public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly DbContext datacontext;
    public EfRepository(DbContext datacontext)
    {
        this.datacontext = datacontext;
    }

    public async Task AddAsync(T entity)
    {            
        await datacontext.Set<T>().AddAsync(entity);
        await datacontext.SaveChangesAsync();
    }

    public async Task<T?> GetAsync(long id)
    {
        T? entity = null;

        entity = await datacontext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

        return entity;
    }
}
```

В файл appsettings.json добавил строку подключения к БД:
```cs
"ConnectionStrings": {
    "db": "Host=localhost;Port=5432;Database=customer;Username=postgres;Password=admin"
  }
```

В файл Program.cs добавил работу с БД с помошью EntityFrameworkCore:

```cs
// добавляем DBContext
builder.Services.AddDbContext<CustomerContext>(x =>
{
    x.UseNpgsql(builder.Configuration.GetConnectionString("db"));
    // Добавляем Naming Convention
    x.UseSnakeCaseNamingConvention();   
});

builder.Services.AddScoped(typeof(DbContext), typeof(CustomerContext));
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
```

Реализовал контроллер следующим образом:
```cs
[Route("customers")]
public class CustomerController : Controller
{
    private readonly IRepository<Customer> customerRepository;
    public CustomerController(IRepository<Customer> customerRepository)
    {
        this.customerRepository = customerRepository;
    }
    [HttpGet("{id:long}")]
    public async Task <IActionResult> GetCustomerAsync([FromRoute] long id)
    {
        //ResponseType type = ResponseType.Success;
        var user = await customerRepository.GetAsync(id);
        if (user == null)
        {
            return NotFound("Customer not found!");
        }
        return  Ok(user);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateCustomerAsync([FromBody] Customer customer)
    {
        var user = await customerRepository.GetAsync(customer.Id);
        if (user != null)
        {                
            return StatusCode(409, "Customer already exists!");
        }
        await customerRepository.AddAsync(customer);
        return Ok(customer.Id);
    }
}
```

### 2. Доработать консольное приложение, чтобы оно удовлетворяло следующим требованиям:

#### 2.1 Принимает с консоли ID "Клиента", запрашивает его с сервера и отображает его данные по пользователю;

#### 2.2 Генерирует случайным образом данные для содания нового "Клиента" на сервере;

#### 2.3 Отправляет данные, созданные в пункте 2.2., на сервер;

#### 2.4 По полученному ID от сервера запросить созданного пользователя с сервера и вывести на экран.