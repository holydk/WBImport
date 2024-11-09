using System.Text.Json;

namespace WBImport
{
    internal class ConsoleWBSupplyImporter
    {
        public async Task ImportAsync(string supplyId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(supplyId);

            // 1. Получить поставку по ID
            // 2. Получить сборочные задания в поставке
            // 3. Создать заказ и отгрузку MS

            var supply = await GetAsync<WBSupply>($"{Defaults.WB_MARKETPLACE_BASE_URL}/supplies/{supplyId}");
            if (supply == null)
            {
                Console.WriteLine("Поставка не найдена.");
                return;
            }

            Console.WriteLine($"Дата поставки: {supply.CreatedAt}");
            Console.WriteLine();

            var supplyOrders = await GetAsync<WBSupplyOrder>($"{Defaults.WB_MARKETPLACE_BASE_URL}/supplies/{supplyId}/orders");
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

                var orderMeta = await GetAsync<WBOrderMeta>($"{Defaults.WB_MARKETPLACE_BASE_URL}/orders/{order.Id}/meta");
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

        #region Utilities

        private static async Task<T> GetAsync<T>(string path)
        {
            var accessToken = Settings.Default?.Wildberries?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Wildberries access token was empty.");

            using var request = new HttpRequestMessage(HttpMethod.Get, path);

            request.Headers.Add("Authorization", accessToken);
            request.Headers.Add("Accept", "application/json");

            using var response = await Defaults.HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        #endregion Utilities
    }
}