using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var subjects = await _subjectService.GetAllSubjectsAsync();
        return Ok(subjects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var subject = await _subjectService.GetSubjectByIdAsync(id);
        if (subject == null) return NotFound(new { message = "Subject not found" });
        return Ok(subject);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Subject subject)
    {
        var created = await _subjectService.CreateSubjectAsync(subject);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Subject subject)
    {
        subject.Id = id;
        await _subjectService.UpdateSubjectAsync(subject);
        return Ok(new { message = "Subject updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _subjectService.DeleteSubjectAsync(id);
        return Ok(new { message = "Subject deleted successfully" });
    }
}
