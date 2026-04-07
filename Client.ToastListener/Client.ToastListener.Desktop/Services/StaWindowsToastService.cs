using Avalonia.Controls;
using Client.ToastListener.Services;
using Serilog;
using System.Drawing;
using System.Windows.Forms;

namespace Client.ToastListener.Desktop.Services;

/// <summary>
/// Показ toast через выделенный STA-поток с циклом сообщений WinForms (без Avalonia).
/// Используется в режиме Windows Service, где нет UI-потока Avalonia.
/// </summary>
public sealed class StaWindowsToastService : IToastService, IDisposable
{
    private readonly ManualResetEventSlim _staReady = new(false);
    private Form? _hiddenForm;
    private Thread? _staThread;
    private volatile bool _disposed;

    /// <summary>
    /// Создаёт сервис и блокируется до готовности STA-потока и инициализации Compat.
    /// </summary>
    public StaWindowsToastService()
    {
        _staThread = new Thread(StaThreadMain)
        {
            IsBackground = true,
            Name = "PcManagerToastSta"
        };
        _staThread.SetApartmentState(ApartmentState.STA);
        _staThread.Start();
        if (!_staReady.Wait(TimeSpan.FromSeconds(30)))
        {
            throw new InvalidOperationException("Не удалось инициализировать STA-поток для toast за 30 с.");
        }
    }

    private void StaThreadMain()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        _hiddenForm = new Form
        {
            Text = ToastListenerApplication.WindowTitle,
            ShowInTaskbar = false,
            Visible = false,
            WindowState = FormWindowState.Minimized,
            Size = new Size(1, 1),
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };

        _ = _hiddenForm.Handle;
        ToastNotificationCompatBootstrap.TryInitialize();
        _staReady.Set();

        Application.Run(_hiddenForm);
    }

    /// <inheritdoc />
    public void Attach(TopLevel topLevel)
    {
        // Не используется без Avalonia.
    }

    /// <inheritdoc />
    public void ShowInformation(string title, string? message = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var form = _hiddenForm;
        if (form is null || !form.IsHandleCreated)
        {
            Log.Error("STA-форма для toast не готова.");
            return;
        }

        void ShowCore()
        {
            ToastNotificationPresenter.Show(title, message);
        }

        if (form.InvokeRequired)
        {
            form.Invoke(ShowCore);
        }
        else
        {
            ShowCore();
        }
    }

    /// <summary>
    /// Освобождает STA-поток и форму.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            var form = _hiddenForm;
            if (form is { IsHandleCreated: true, IsDisposed: false })
            {
                form.BeginInvoke(() =>
                {
                    try
                    {
                        form.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "Закрытие скрытой формы toast.");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Освобождение StaWindowsToastService.");
        }

        _staThread?.Join(TimeSpan.FromSeconds(5));
        _staThread = null;
        _hiddenForm = null;
        _staReady.Dispose();
    }
}
