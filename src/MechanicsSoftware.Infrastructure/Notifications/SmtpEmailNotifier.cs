using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Domain.ValueObjects;

namespace MechanicsSoftware.Infrastructure.Notifications;

public sealed class SmtpEmailNotifier : IEmailNotifier
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _pass;
    private readonly string _from;

    public SmtpEmailNotifier(IConfiguration configuration)
    {
        _host = configuration["SMTP_HOST"]
            ?? throw new InvalidOperationException(
                "SMTP host not configured. Set the 'SMTP_HOST' environment variable.");

        var portRaw = configuration["SMTP_PORT"]
            ?? throw new InvalidOperationException(
                "SMTP port not configured. Set the 'SMTP_PORT' environment variable.");

        if (!int.TryParse(portRaw, out _port))
            throw new InvalidOperationException(
                $"Invalid value for 'SMTP_PORT': '{portRaw}'. Expected a numeric port (e.g. 587).");

        _user = configuration["SMTP_USER"]
            ?? throw new InvalidOperationException(
                "SMTP user not configured. Set the 'SMTP_USER' environment variable.");

        _pass = configuration["SMTP_PASS"]
            ?? throw new InvalidOperationException(
                "SMTP password not configured. Set the 'SMTP_PASS' environment variable.");

        _from = configuration["SMTP_FROM"]
            ?? throw new InvalidOperationException(
                "SMTP sender address not configured. Set the 'SMTP_FROM' environment variable.");
    }

    public async Task SendStatusChangedAsync(
        string toEmail,
        string customerName,
        Guid serviceOrderId,
        ServiceOrderStatus.Status newStatus,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Atualização da sua Ordem de Serviço #{serviceOrderId}";
        var body = BuildBody(customerName, serviceOrderId, newStatus);

        using var client = new SmtpClient(_host, _port)
        {
            Credentials = new NetworkCredential(_user, _pass),
            EnableSsl = true
        };

        using var message = new MailMessage(
            from: new MailAddress(_from),
            to: new MailAddress(toEmail))
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private static string BuildBody(
        string customerName,
        Guid serviceOrderId,
        ServiceOrderStatus.Status newStatus)
    {
        var statusLabel = new ServiceOrderStatus(newStatus).ToString();

        return $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="UTF-8" /></head>
            <body style="font-family:Arial,sans-serif;background:#f4f4f4;padding:32px;">
              <table width="600" cellpadding="0" cellspacing="0"
                     style="background:#ffffff;border-radius:8px;padding:32px;margin:auto;">
                <tr>
                  <td>
                    <h2 style="color:#1a1a1a;margin-top:0;">
                      Atualização da sua Ordem de Serviço
                    </h2>
                    <p style="color:#333;">Olá, <strong>{customerName}</strong>!</p>
                    <p style="color:#333;">
                      Sua Ordem de Serviço <strong>#{serviceOrderId}</strong>
                      teve o status atualizado.
                    </p>
                    <p style="background:#f0f4ff;border-left:4px solid #4f6ef7;
                               padding:12px 16px;border-radius:4px;color:#1a1a1a;">
                      Novo status: <strong>{statusLabel}</strong>
                    </p>
                    <p style="color:#555;font-size:14px;">
                      Caso tenha dúvidas, entre em contato conosco.
                    </p>
                    <hr style="border:none;border-top:1px solid #eee;margin:24px 0;" />
                    <p style="color:#999;font-size:12px;margin:0;">
                      Equipe de Atendimento
                    </p>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
