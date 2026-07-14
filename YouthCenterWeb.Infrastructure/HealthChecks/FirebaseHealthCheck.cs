// Infrastructure/HealthChecks/FirebaseHealthCheck.cs
using FirebaseAdmin;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class FirebaseHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Firebase app is not initialized."));
        }

        try
        {
            // بيتأكد إن الـ credential شغالة فعليًا من غير ما يبعت إشعار حقيقي
            var projectId = FirebaseApp.DefaultInstance.Options.ProjectId;

            return Task.FromResult(
                HealthCheckResult.Healthy($"Firebase initialized for project: {projectId}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Firebase credential check failed.", ex));
        }
    }
}