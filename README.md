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

Добавил миграцию InitialCreate и обновил БД:

```
Add-Migration InitialCreate
Update-Database
```

### 2. Доработать консольное приложение, чтобы оно удовлетворяло следующим требованиям:

#### 2.1 Принимает с консоли ID "Клиента", запрашивает его с сервера и отображает его данные по пользователю;

Для решения это задачи создал метод GetCustomerByIdFromServer(HttpClient client, long id):
```cs
static async Task<bool> GetCustomerByIdFromServer(HttpClient client, long id)
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    HttpResponseMessage response = await client.GetAsync("customers/" + id.ToString());
    if (response.StatusCode == HttpStatusCode.OK) // 200
    {
        var json = await response.Content.ReadAsStringAsync();
        var customer = JsonConvert.DeserializeObject<Customer>(json);

        Console.WriteLine("На сервере найден клиент: id {0}; FirstName {1}; LastName {2}", customer.Id, customer.Firstname, customer.Lastname);
    }
    else if (response.StatusCode == HttpStatusCode.NotFound) // 404
    {
        Console.WriteLine("На сервере нет клиента с ID = " + id.ToString());
        return false;
    }
    else
    {
        Console.WriteLine("Некорректный запрос!");
        return false;
    }
    return true;
}
```

Вызов метода и чтение номера с консоли происходит здесь:

```cs
Console.WriteLine("Введите ID клиента");
string customerIdStr = Console.ReadLine();

using (var client = new HttpClient { BaseAddress = new Uri("http://localhost:5072") })
{
    if (Int64.TryParse(customerIdStr, out var customerId))
    {
        await GetCustomerByIdFromServer(client, customerId);
    }
    else Console.WriteLine("Введен некорректный ID клиента!");    
}
```
#### 2.2 Генерирует случайным образом данные для содания нового "Клиента" на сервере;

Генерацию случайного клиента сделал таким образом:

```cs
static CustomerCreateRequest RandomCustomer()
{
    CustomerCreateRequest customerCreateRequest = new CustomerCreateRequest(GenWord(5), GenWord(6));
    return customerCreateRequest;   
}

static string GenWord(int len)
{
    // Создаем массив букв, которые мы будем использовать.
    char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    //Создание объекта для генерации чисел
    Random rnd = new Random();

    string word = "";
    for (int j = 1; j <= len; j++)
    {
        // Выбор случайного числа от 0 до 25
        // для выбора буквы из массива букв.
        int letter_num = rnd.Next(0, len - 1);

        // Добавить письмо.
        word += letters[letter_num];
    }

    return word;
}
```

#### 2.3 Отправляет данные, созданные в пункте 2.2., на сервер;
Отправка данных реализована в функции PostCustomerToServer(HttpClient client)
```cs
static async Task<(bool, long)> PostCustomerToServer(HttpClient client)
{
    bool res = false;
    long customerId = 0; 
    CustomerCreateRequest randomCustomer = RandomCustomer();
    JsonContent content = JsonContent.Create(randomCustomer);

    Console.WriteLine("Запрос на создание в БД сервера клиента:  FirstName {0}; LastName {1}", randomCustomer.Firstname, randomCustomer.Lastname);
    HttpResponseMessage response = await client.PostAsync("customers", content);

    if (response.StatusCode == HttpStatusCode.OK)
    {
        var json = await response.Content.ReadAsStringAsync();
        customerId = JsonConvert.DeserializeObject<long>(json);
        res = true;
    }
    return (res, customerId);
}
```

#### 2.4 По полученному ID от сервера запросить созданного пользователя с сервера и вывести на экран.
После отправки запроса на создание клиента на сервере, получаем ID созданного пользователя и опять вызываем функцию GetCustomerByIdFromServer(HttpClient client, long id):
```cs
Console.WriteLine("Введите ID клиента");
string customerIdStr = Console.ReadLine();

using (var client = new HttpClient { BaseAddress = new Uri("http://localhost:5072") })
{
    if (Int64.TryParse(customerIdStr, out var customerId))
    {

        await GetCustomerByIdFromServer(client, customerId);

    }
    else Console.WriteLine("Введен некорректный ID клиента!");

    var postres = await PostCustomerToServer(client);

    if (postres.Item1)
    {
        await GetCustomerByIdFromServer(client, postres.Item2);
    }
    else Console.WriteLine("Ошибка создания клиента!");
}
Console.ReadLine();
```

Результат работы программы:
<image src="images/result.png" alt="result">