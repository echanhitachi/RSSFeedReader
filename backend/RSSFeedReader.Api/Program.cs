using RSSFeedReader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// Least-privilege CORS: only the frontend's actual configured origins, per constitution Principle I.
var frontendOrigins = builder.Configuration.GetSection("FrontendOrigins").Get<string[]>()
    ?? ["http://localhost:5274", "https://localhost:7165"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddSingleton<ISubscriptionService, InMemorySubscriptionService>();
builder.Services.AddHttpClient(nameof(FeedValidationService));
builder.Services.AddSingleton<IFeedValidationService, FeedValidationService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);

app.MapPost("/api/subscriptions", async (AddSubscriptionRequest request, ISubscriptionService subscriptions, IFeedValidationService feedValidation) =>
{
    var validation = await feedValidation.ValidateAsync(request.Url);
    if (!validation.IsValid)
    {
        return Results.BadRequest(new { error = validation.ErrorMessage });
    }

    var subscription = subscriptions.Add(request.Url);
    return Results.Created("/api/subscriptions", subscription);
})
.WithName("AddSubscription");

app.MapGet("/api/subscriptions", (ISubscriptionService subscriptions) =>
    Results.Ok(subscriptions.GetAll()))
.WithName("GetSubscriptions");

app.MapDelete("/api/subscriptions/{id:guid}", (Guid id, ISubscriptionService subscriptions) =>
{
    subscriptions.Remove(id);
    return Results.NoContent();
})
.WithName("RemoveSubscription");

app.Run();

record AddSubscriptionRequest(string Url);

