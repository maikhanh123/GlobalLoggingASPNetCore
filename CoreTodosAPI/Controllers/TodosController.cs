using CoreTodosAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CoreTodosAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Todos")]
    [Authorize()]
    public class TodosController : Controller
    {
        private ToDoDbContext _db;

        public TodosController(ToDoDbContext context) // from dependency injection
        {
            _db = context;
        }

        [HttpGet]
        public IEnumerable<ToDoItem> Get()
        {
            var user = User;
            
            return _db.ToDoItems                
                .ToList(); ;
        }

        [HttpGet]
        [Route("Error")]
        public IEnumerable<ToDoItem> GetWithError()
        {
            var user = User;

            return _db.ToDoItems
                .Include("garbage")
                .ToList(); ;
        }

        [HttpGet("{id}")]
        public ToDoItem Get(int id)
        {
            return _db.ToDoItems.Find(id);
        }

        [HttpPost]
        public int Post([FromBody]ToDoItem item)
        {
            _db.ToDoItems.Add(item);
            _db.SaveChanges();
            return item.Id;
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]ToDoItem item)
        {
            var itemToUpdate = _db.ToDoItems.Find(id);
            itemToUpdate.Item = item.Item;
            itemToUpdate.Completed = item.Completed;
            _db.SaveChanges();
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var itemToRemove = _db.ToDoItems.Find(id);
            _db.ToDoItems.Remove(itemToRemove);
        }
    }
}