using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using log4net;
using System.Xml.Serialization;
using log4net.Config;
using System.Threading;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;


#region Task_2-3

public class Order
{
	public int Id { get; set; }
	public int TableNumber { get; set; }
	public List<string> Items { get; set; }
	public decimal TotalAmount { get; set; }

	public Order(int id, int tableNumber, List<string> items, decimal totalAmount)
	{
		Id = id;
		TableNumber = tableNumber;
		Items = items;
		TotalAmount = totalAmount;
	}
}

public class BurgerKing
{
	public class Table
	{
		public int Id { get; init; }
		public int Places { get; init; }
		public bool IsReserve { get; set; }
	}

	public List<Table> Tables { get; set; } = new();
	private Queue<Order> Orders { get; set; } = new();
	private List<(string, decimal)> Menu { get; set; } = new();
	public string? OrdersFilePath { get; set; }

	public BurgerKing()
	{
		Initialization();
	}
	public BurgerKing(string ordersFilePath)
	{
		OrdersFilePath = ordersFilePath ?? throw new ArgumentNullException(nameof(ordersFilePath));
		Initialization();
	}

	public void AddToMenu(string name, decimal price)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Menu item name cannot be empty");
		if (price <= 0)
			throw new ArgumentException("Price must be greater than zero");
		Menu.Add((name, price));
	}
	public void RemoveFromMenu(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Menu item name cannot be empty");
		int index = Menu.FindIndex(i => i.Item1 == name);
		if (index == -1)
			throw new InvalidOperationException($"Menu item '{name}' not found");
		Menu.RemoveAt(index);
	}
	public bool ReserveTable(int id)
	{
		if (id <= 0)
			throw new ArgumentException("Table ID must be positive");

		var table = Tables.Find(t => t.Id == id);
		if (table == null)
			throw new KeyNotFoundException($"Table with ID {id} does not exist");

		table.IsReserve = !table.IsReserve;
		return true;
	}
	public void AddOrder(Order order)
	{
		if (order == null)
			throw new ArgumentNullException(nameof(order));
		if (order.Id <= 0)
			throw new ArgumentException("Order ID must be positive");
		if (order.TotalAmount <= 0)
			throw new ArgumentException("Total amount must be greater than 0");
		if (order.Items == null || order.Items.Count == 0)
			throw new ArgumentException("Order must contain at least one item");
		Orders.Enqueue(order);
	}
	public bool EditOrder(int id, int? newTableNumber = null, List<string> newItems = null, decimal? newTotalAmount = null)
	{
		if (id <= 0)
			throw new ArgumentException("Order ID must be positive");

		var order = Orders.FirstOrDefault(o => o.Id == id);
		if (order == null)
			throw new KeyNotFoundException($"Order with ID {id} not found");

		if (newTableNumber.HasValue && newTableNumber <= 0)
			throw new ArgumentException("Table number must be positive");

		if (newTotalAmount.HasValue && newTotalAmount <= 0)
			throw new ArgumentException("Total amount must be greater than zero");

		if (newTableNumber.HasValue)
			order.TableNumber = newTableNumber.Value;
		if (newItems != null && newItems.Count > 0)
			order.Items = newItems;
		if (newTotalAmount.HasValue)
			order.TotalAmount = newTotalAmount.Value;

		return true;
	}
	public bool RemoveLastOrder()
	{
		if (Orders.Count == 0)
			throw new InvalidOperationException("No orders to remove");

		Orders.Dequeue();
		return true;
	}

	public void Initialization()
	{
		if (string.IsNullOrWhiteSpace(OrdersFilePath))
		{
			OrdersFilePath = "orders.json";
		}

		if (!File.Exists(OrdersFilePath))
			return;

		using FileStream fs = new FileStream(OrdersFilePath, FileMode.Open, FileAccess.Read);
		using StreamReader reader = new StreamReader(fs);
		string json_data = reader.ReadToEnd();
		if (string.IsNullOrWhiteSpace(json_data))
			return;

		var orders = JsonSerializer.Deserialize<Order[]>(json_data);
		if (orders == null)
			throw new InvalidOperationException("Failed to deserialize orders from file");

		foreach (var order in orders)
		{
			if (order != null)
			{
				Orders.Enqueue(order);
			}
		}
	}
	public void Preservation()
	{
		if (string.IsNullOrWhiteSpace(OrdersFilePath))
			throw new InvalidOperationException("OrdersFilePath is not set");

		if (Orders.Count == 0)
			throw new InvalidOperationException("No orders to save");

		using FileStream fs = new FileStream(OrdersFilePath, FileMode.Create);
		JsonSerializerOptions options = new()
		{
			WriteIndented = true
		};
		JsonSerializer.SerializeAsync(fs, Orders, options);
	}

}

#endregion


internal class Program
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    static async Task Main(string[] args)
    {

        #region Task_1
        AdoptMeGame newGame = new();
		newGame.Play();

		#endregion

		#region Task_2-3

		var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        try
        {
            BurgerKing bk = new BurgerKing("orders.json");

            bk.AddToMenu("Cheeseburger", 4.99m);
            bk.AddToMenu("", -1);

            Order order = new(1, 2, new List<string> { "Cheeseburger" }, 4.99m);
            bk.AddOrder(order);

            bk.Preservation();
        }
        catch (Exception ex)
        {
            log.Error($"\nDate - {DateTime.Now.Date.ToShortDateString()}" +
				$"\nTime - { DateTime.Now.ToString("HH:mm:ss")}" +
                $"\nLog level - { LogLevel.Error}" +
                $"\nMessage - { ex.Message}" +
                $"\nStackTrace - { ex.StackTrace}" +
                $"\nTargetSite - { ex.TargetSite}" +
                $"\nDetails: User try to divide by zero." +
                $"\n......\n......\n");
            Console.WriteLine("Error logged to file.");
        }

        #endregion

    }

}