using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using MechanicsSoftware.Application.Abstractions;

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
        string newStatus,
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
            IsBodyHtml = false
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private static string BuildBody(string customerName, Guid serviceOrderId, string newStatus) =>
        $"""
        Olá, {customerName}!

        Sua Ordem de Serviço #{serviceOrderId} teve o status atualizado.

        Novo status: {newStatus}

        Caso tenha dúvidas, entre em contato conosco.

        Atenciosamente,
        Equipe de Atendimento
        """;
}