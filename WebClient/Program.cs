using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HttpClientSample
{
    public class TodoItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }

    class Program
    {
        static HttpClient client = new HttpClient();

        static void ShowTodoItem(TodoItem todoItem)
        {
            Console.WriteLine($"Name: {todoItem.Name}\tIs Complete: " +
                $"{todoItem.IsComplete}");
        }

        static async Task<Uri> CreateTodoAsync(TodoItem todoItem)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "api/todo", todoItem);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static async Task<TodoItem> GetTodoAsync(string path)
        {
            TodoItem todoItem = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                todoItem = await response.Content.ReadAsAsync<TodoItem>();
            }
            return todoItem;
        }

        static async Task<TodoItem> UpdateTodoAsync(TodoItem todoItem)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"api/todo/{todoItem.Id}", todoItem);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated todoItem from the response body.
            todoItem = await response.Content.ReadAsAsync<TodoItem>();
            return todoItem;
        }

        static async Task<HttpStatusCode> DeleteTodoAsync(long id)
        {
            HttpResponseMessage response = await client.DeleteAsync(
                $"api/todo/{id}");
            return response.StatusCode;
        }

        static void Main()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            // Update port # in the following line.
            // use http://localhost.fiddler:5000/ to debug using fiddler
            client.BaseAddress = new Uri("http://localhost:5000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // Create a new todoItem
                TodoItem todoItem = new TodoItem
                {
                    Id = 0,
                    Name = "Gizmo",
                    IsComplete = false
                };

                var url = await CreateTodoAsync(todoItem);
                Console.WriteLine($"Created at {url}");

                // Get the todoItem
                todoItem = await GetTodoAsync(url.PathAndQuery);
                ShowTodoItem(todoItem);

                // Update the todoItem
                Console.WriteLine("Updating is Complete...");
                todoItem.IsComplete = true;
                await UpdateTodoAsync(todoItem);

                // Get the updated todoItem
                todoItem = await GetTodoAsync(url.PathAndQuery);
                ShowTodoItem(todoItem);

                // Delete the todoItem
                var statusCode = await DeleteTodoAsync(todoItem.Id);
                Console.WriteLine($"Deleted (HTTP Status = {(int)statusCode})");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}