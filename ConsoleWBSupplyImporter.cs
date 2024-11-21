namespace WBImport
{
    internal class ConsoleWBSupplyImporter
    {
        public async Task ImportAsync(string supplyId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(supplyId);

            var supply = await WBClient.GetAsync<WBSupply>($"{Defaults.WB_MARKETPLACE_BASE_URL}/supplies/{supplyId}");
            if (supply == null)
            {
                Console.WriteLine("Поставка не найдена.");
                return;
            }

            Console.WriteLine($"Дата поставки: {supply.CreatedAt}");
            Console.WriteLine();

            var supplyOrders = await WBClient.GetAsync<WBSupplyOrder>($"{Defaults.WB_MARKETPLACE_BASE_URL}/supplies/{supplyId}/orders");
            var orders = supplyOrders.Orders;
            if (orders == null || orders.Length == 0)
                return;

            foreach (var order in orders)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(order.Article);
                Console.ResetColor();
                Console.WriteLine($"Номер заказа: {order.Id}");

                if (order.Barcodes?.Any() == true)
                {
                    foreach (var barcode in order.Barcodes)
                        Console.WriteLine($"Штрих-код: {barcode}");
                }

                var orderMeta = await WBClient.GetAsync<WBOrderMeta>($"{Defaults.WB_MARKETPLACE_BASE_URL}/orders/{order.Id}/meta");
                if (orderMeta != null)
                {
                    var sGrinValues = orderMeta.Meta?.SGtin?.Value;
                    if (sGrinValues?.Any() == true)
                    {
                        foreach (var code in sGrinValues)
                            Console.WriteLine($"Маркировка: {code}");
                    }
                }

                Console.WriteLine();
            }
        }
    }
}