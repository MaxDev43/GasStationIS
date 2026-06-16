using System;
using System.Windows;
using System.Windows.Threading;
using GasStationIS.Core;

namespace GasStationIS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Глобальный обработчик неперехваченных исключений UI-потока
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            // Глобальный обработчик для фоновых потоков
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                Logger.Info("═══ Запуск приложения «Информационная система сети АЗС» ═══");
                DatabaseService.InitializeDatabase();
                Logger.Info("БД готова. Открываем окно входа.");
            }
            catch (Exception ex)
            {
                Logger.Error("Критическая ошибка при инициализации БД", ex);
                MessageBox.Show(
                    $"Ошибка при инициализации базы данных:\n\n{ex.Message}\n\n" +
                    $"Проверьте лог-файл в папке Logs\\.",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("═══ Завершение приложения ═══");
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error("Необработанное исключение UI", e.Exception);
            MessageBox.Show(
                $"Произошла непредвиденная ошибка:\n\n{e.Exception.Message}\n\n" +
                "Приложение продолжит работу. Подробности — в Logs\\.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.Error("Необработанное исключение (фоновый поток)", ex);
        }
    }
}
