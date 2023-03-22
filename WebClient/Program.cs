using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebClient;


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
}
Console.ReadLine();

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
