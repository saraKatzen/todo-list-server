
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
builder.Services.AddDbContext<ToDoDbContext>();

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
    // 1. מוודאים שמי ששלח את הבקשה לא התבלבל בין הכתובת לתוכן
    if (id != item.Id)
    {
        return Results.BadRequest();
    }

    // 2. הבדיקה החכמה שלך: האם המשימה בכלל קיימת במסד הנתונים?
    var existingItem = context.Items.Find(id);
    if (existingItem is null)
    {
        return Results.NotFound(); // מחזיר שגיאת 404 אם אין משימה כזו
    }

    // 3. אם מצאנו אותה - מעדכנים את הנתונים שלה
    // שימי לב: אני מניח שיש למשימה שדות כמו Name ו-IsComplete. 
    // אם קראת להם אחרת, תשני את השמות בהתאם.
    existingItem.Name = item.Name;
    existingItem.IsComplete = item.IsComplete;

    // 4. שומרים את השינויים
    context.SaveChanges();

    return Results.NoContent();
});
app.MapDelete("/items/{id}", (int id, ToDoDbContext context) =>
{
    // 1. מחפשים את המשימה במסד הנתונים לפי המספר שלה
    var item = context.Items.Find(id);

    // 2. בודקים אם היא בכלל קיימת
    if (item is null)
    {
        return Results.NotFound(); // מחזיר שגיאת 404: "לא נמצא"
    }

    // 3. אם היא קיימת - מוחקים ושומרים
    context.Items.Remove(item);
    context.SaveChanges();

    return Results.NoContent(); // מחזיר שהכל עבר בהצלחה ואין מה להוסיף
});
app.MapGet("/", () => "Hello World!");

app.Run();
