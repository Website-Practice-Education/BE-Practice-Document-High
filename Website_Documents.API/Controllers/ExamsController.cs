using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var exams = await _examService.GetAllExamsAsync();
        return Ok(exams);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var exam = await _examService.GetExamByIdAsync(id);
        if (exam == null) return NotFound(new { message = "Exam not found" });
        return Ok(exam);
    }

    [HttpGet("subject/{subjectId}")]
    public async Task<IActionResult> GetBySubjectId(int subjectId)
    {
        var exams = await _examService.GetExamsBySubjectIdAsync(subjectId);
        return Ok(exams);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Exam exam)
    {
        var created = await _examService.CreateExamAsync(exam);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Exam exam)
    {
        exam.Id = id;
        await _examService.UpdateExamAsync(exam);
        return Ok(new { message = "Exam updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _examService.DeleteExamAsync(id);
        return Ok(new { message = "Exam deleted successfully" });
    }
}
