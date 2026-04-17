
using Microsoft.EntityFrameworkCore;
using TodoApi;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration["ToDoDB"],
        ServerVersion.AutoDetect(builder.Configuration["ToDoDB"])
    ));
    Console.WriteLine("TO DO DB:");
Console.WriteLine(builder.Configuration["ToDoDB"]);
var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/items", (ToDoDbContext context) =>
{
    return context.Items.ToList();
});
app.MapPost("/items", (ToDoDbContext context, Item item) =>
{
    context.Items.Add(item);
    context.SaveChanges();
    return Results.Created($"/items/{item.Id}", item);
});
app.MapPut("/items/{id}", (int id, Item item, ToDoDbContext context) =>
{
    // 1. ������� ��� ���� �� ����� �� ������ ��� ������ �����
    if (id != item.Id)
    {
        return Results.BadRequest();
    }

    // 2. ������ ����� ���: ��� ������ ���� ����� ���� �������?
    var existingItem = context.Items.Find(id);
    if (existingItem is null)
    {
        return Results.NotFound(); // ����� ����� 404 �� ��� ����� ���
    }

    // 3. �� ����� ���� - ������� �� ������� ���
    // ���� ��: ��� ���� ��� ������ ���� ��� Name �-IsComplete. 
    // �� ���� ��� ����, ���� �� ����� �����.
    existingItem.Name = item.Name;
    existingItem.IsComplete = item.IsComplete;

    // 4. ������ �� ��������
    context.SaveChanges();

    return Results.NoContent();
});
app.MapDelete("/items/{id}", (int id, ToDoDbContext context) =>
{
    // 1. ������ �� ������ ���� ������� ��� ����� ���
    var item = context.Items.Find(id);

    // 2. ������ �� ��� ���� �����
    if (item is null)
    {
        return Results.NotFound(); // ����� ����� 404: "�� ����"
    }

    // 3. �� ��� ����� - ������ �������
    context.Items.Remove(item);
    context.SaveChanges();

    return Results.NoContent(); // ����� ���� ��� ������ ���� �� ������
});
app.MapGet("/", () => "Hello World!");

app.Run();
