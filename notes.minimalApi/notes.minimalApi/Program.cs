var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//
builder.Services.AddSingleton<IBookRepository, BookRepository>();
builder.Services.AddHealthChecks();
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/books", (IBookRepository bookRepository) => 
{
    return bookRepository.GetAll();
}).WithName("GetAllBooks");

app.MapGet("/books/{id}", (Guid id, IBookRepository bookRepository) =>
{
    var book =  bookRepository.GetById(id);
    return book is not null ? Results.Ok(book) : Results.NotFound();

}).WithName("GetBook");

app.MapPost("/books", (string title, IBookRepository bookRepository) =>
{
    var book = bookRepository.Create(title);
    return book is not null ? Results.Created($"/books/{book.Id}", book) : Results.StatusCode(500);

}).WithName("CreateBook");

app.MapPut("/book", (Book book, IBookRepository bookRepository) =>
{
    if (bookRepository.GetById(book.Id) is null)
    {
        return Results.NotFound();
    }

    bookRepository.Update(book);
    return Results.Ok(book);
    
}).WithName("UpdateBook");

app.MapDelete("/book/{id}", (Guid id, IBookRepository bookRepository) =>
{
    bookRepository.Delete(id);
    return Results.Ok();
}).WithName("DeleteBook");


app.MapHealthChecks("/healthz");
app.Run();

#region Entities

internal class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }

}

internal interface IBookRepository
{
    Book? Create(string title);
    void Delete(Guid id);
    List<Book> GetAll();
    Book? GetById(Guid id);
    void Update(Book book);
}

internal class BookRepository : IBookRepository
{
    private readonly Dictionary<Guid, Book> _books = new Dictionary<Guid, Book>();

    public Book? Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }
        
        Book book = new Book { Id = Guid.NewGuid(), Title = title};
        _books.Add(book.Id, book);
        return book;
    }

    public void Update(Book book)
    {
        if (_books[book.Id] is null)
        {
            return;
        }
        _books[book.Id] = book;
    }

    public void Delete(Guid id)
    {
        _books.Remove(id);
    }

    public Book? GetById(Guid id)
    {
        return _books.ContainsKey(id)? _books[id]: null;
    }

    public List<Book> GetAll()
    {
        return _books.Values.ToList();
    }
}

#endregion
