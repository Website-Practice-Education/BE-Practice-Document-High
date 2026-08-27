using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var questions = await _questionService.GetAllQuestionsAsync();
        return Ok(questions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var question = await _questionService.GetQuestionDetailAsync(id);
        if (question == null) return NotFound(new { message = "Question not found" });
        return Ok(question);
    }

    [HttpGet("subject/{subjectId}")]
    public async Task<IActionResult> GetBySubjectId(int subjectId)
    {
        var questions = await _questionService.GetQuestionsBySubjectIdAsync(subjectId);
        return Ok(questions);
    }

    [HttpGet("lesson/{lessonId}")]
    public async Task<IActionResult> GetByLessonId(int lessonId)
    {
        var questions = await _questionService.GetQuestionsByLessonIdAsync(lessonId);
        return Ok(questions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
    {
        var question = new Question
        {
            SubjectId = request.SubjectId,
            TopicId = request.TopicId,
            LessonId = request.LessonId,
            QuestionType = request.QuestionType,
            Content = request.Content,
            Explanation = request.Explanation,
            Difficulty = request.Difficulty,
            Year = request.Year,
            Source = request.Source,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            IsActive = true,
            QuestionOptions = request.Options.Select(o => new QuestionOption
            {
                OptionKey = o.OptionKey,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        var created = await _questionService.CreateQuestionAsync(question);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] CreateQuestionRequest request)
    {
        var existingQuestion = await _questionService.GetQuestionByIdAsync(id);
        if (existingQuestion == null) return NotFound(new { message = "Question not found" });

        existingQuestion.SubjectId = request.SubjectId;
        existingQuestion.TopicId = request.TopicId;
        existingQuestion.LessonId = request.LessonId;
        existingQuestion.QuestionType = request.QuestionType;
        existingQuestion.Content = request.Content;
        existingQuestion.Explanation = request.Explanation;
        existingQuestion.Difficulty = request.Difficulty;
        existingQuestion.Year = request.Year;
        existingQuestion.Source = request.Source;
        existingQuestion.FileUrl = request.FileUrl;
        existingQuestion.FileType = request.FileType;

        await _questionService.UpdateQuestionAsync(existingQuestion);
        return Ok(new { message = "Question updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _questionService.DeleteQuestionAsync(id);
        return Ok(new { message = "Question deleted successfully" });
    }
}
