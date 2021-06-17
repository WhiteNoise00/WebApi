﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Test.Models;

namespace Test.Controllers
{
    public class HomeController : Controller
    {
        private TaskContext db;
        public HomeController(TaskContext context)
        {
            db = context;
        }

        public async Task<IActionResult> ViewEmployees(string name, int? id) 
        {
           IQueryable<Employee> em = db.Employees.Include(p => p.Tasks);  
          
            if (id != null && id != 0)
            {
                em = em.Where(e => e.Tasks.Any(t => t.Id == id));
            }

            if (!String.IsNullOrEmpty(name)) 
            {
                em = em.Where(x => EF.Functions.Like(x.Employee_Last_Name, $"%{name}%"));
            }
             
            SelectList tasks = new SelectList(db.Tasks, "Id", "Task_Name");
            ViewBag.TasksList = tasks;          
            return View(em);           
        }

        public async Task<IActionResult> Index()
        {           
            return View(await db.Tasks.ToListAsync());
        }

        public IActionResult CreateTask()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(Models.Task ts)
        {
            db.Tasks.Add(ts);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");           
        }

        public ActionResult CreateEmployee()
        {
            ViewBag.Tasks = db.Tasks.ToList();
            return View();
        }

        [HttpPost]
        public  ActionResult CreateEmployee(Employee em, int[] selectedTasks)
        {
            if (selectedTasks!=null)
            {
                foreach (var c in db.Tasks.Where(co => selectedTasks.Contains(co.Id)))
                {
                    em.Tasks.Add(c);
                }
            }

            db.Employees.Add(em);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> TaskDetails(int? id)
        {
            if (id != null)
            {
                Models.Task ts =  await db.Tasks.Include(e => e.Employees).FirstOrDefaultAsync(e => e.Id == id);
                if (ts != null)
                return View(ts);
            }
            return NotFound();            
        }

        public async Task<IActionResult> EmployeeDetails(int? id)
        {           
           Employee em = await db.Employees.Include(e => e.Tasks).FirstOrDefaultAsync(e => e.Id == id);
            if (em == null) { return NotFound(); } 
            return View(em);
        }

        public async Task<IActionResult> TaskEdit(int? id)
        {           
            if (id != null)
            {
                Models.Task ts = await db.Tasks.FirstOrDefaultAsync(p => p.Id == id);
                if (ts != null)
                return View(ts);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> TaskEdit(Models.Task ts) 
        {          
            db.Tasks.Update(ts);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("TaskDelete")]
        public async Task<IActionResult> TaskConfirmDelete(int? id)
        {
            if (id != null)
            {
                Models.Task ts = await db.Tasks.FirstOrDefaultAsync(p => p.Id == id);
                if (ts != null)
                return View(ts);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> TaskDelete(int? id)
        {
            if (id != null)
            {
                Models.Task ts = new Models.Task { Id = id.Value };
                db.Entry(ts).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public async Task<IActionResult> EmployeeEdit(int? id)
        {                      
             Employee em =  db.Employees.Find(id);
             em = await db.Employees.Include(e => e.Tasks).FirstOrDefaultAsync(e => e.Id == id);
             if (em == null) { return NotFound(); }
             ViewBag.Tasks = db.Tasks.ToList();
             return View(em);
        }
   
        [HttpPost]
        public async Task<IActionResult> EmployeeEdit(Employee em, int[] selectedTasks)
        {
           
            Employee employee = await db.Employees.Include(e => e.Tasks).FirstOrDefaultAsync(e => e.Id == em.Id);

            employee.Employee_Last_Name = em.Employee_Last_Name;
            employee.Employee_Name = em.Employee_Name;
            employee.Employee_Email = em.Employee_Email;
            employee.Employee_Middle_Name = em.Employee_Middle_Name;
            employee.Tasks.Clear();

            if (selectedTasks!=null)
            {
                foreach (var c in db.Tasks.Where( co=> selectedTasks.Contains(co.Id)))
                {
                    employee.Tasks.Add(c);
                }
            }
          
            db.Employees.Update(employee);            
            await db.SaveChangesAsync();          
            return RedirectToAction("ViewEmployees");       
        }

        [HttpGet]
        [ActionName("EmployeeDelete")]
        public async Task<IActionResult> EmployeeConfirmDelete(int? id)
        {
            if (id != null)
            {
                Employee em = await db.Employees.FirstOrDefaultAsync(p => p.Id == id);
                if (em != null)
                return View(em);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeDelete(int? id)
        {
            if (id != null)
            {
                Employee em = new Employee { Id = id.Value };
                db.Entry(em).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public async Task<IActionResult> ViewEmployee(int? id)
        {
            if (id != null)
            {
                Models.Task ts = new Models.Task();
                var tasks = await db.Tasks.Where(b => b.Id == id).ToListAsync();
                ViewBag.Tasks = tasks;               
            }
            return NotFound();
        }

        [HttpGet]
        [ActionName("ViewAllTasksEmployee")]
        public async Task<IActionResult> ViewAllTasksEmployee(int? id)
        {
            if (id != null)
            {
                Models.Task ts = await db.Tasks.FirstOrDefaultAsync(p => p.Id == id);
                if (ts != null)
                return View(ts);
            }
            return NotFound();
        }
    } 
}



