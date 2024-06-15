using DataAccess;
using Entities;
using Microsoft.AspNetCore.Mvc;

namespace StudentResults.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataAccessor _dataAccessor = new();
        private readonly HashSet<string> PassingGrades = new() { "C", "C+", "B", "B+", "A", "A+" };

        public HomeController()
        {
        }

        public IActionResult Index()
        {
            Dictionary<int, string> students = _dataAccessor.GetStudents();
            List<Grade> grades = _dataAccessor.GetGrades();
            List<Course> courses = _dataAccessor.GetCourses();

            List<StudentSummary> studentSummaries = students.Select(student => new StudentSummary(student.Value, new List<CourseResult>(), false, 0)).ToList();

            List<CourseResult> correlatedCourseResults = grades
                .Join(courses, grade => grade.CourseId, course => course.CourseId, (grade, course) =>
                    new CourseResult(course.CourseName ?? "N/A", grade.GradeValue ?? "N/A", grade.StudentId))
                .ToList();

            foreach (var studentSummary in studentSummaries)
            {
                studentSummary.CourseResult = correlatedCourseResults
                    .Where(courseResult => courseResult.StudentId == students.FirstOrDefault(s => s.Value == studentSummary.StudentName).Key)
                    .ToList();

                int gradeCounter = 0;

                foreach (var courseResult in studentSummary.CourseResult)
                {
                    if (PassingGrades.Contains(courseResult.Grade))
                    {
                        gradeCounter++;
                    }
                }

                studentSummary.IsPassing = gradeCounter >= 3;
            }

            studentSummaries = FormatTable(studentSummaries);
            return View(studentSummaries);
        }

        protected List<StudentSummary> FormatTable(List<StudentSummary> studentSummaries)
        {
            int maxCourseResults = studentSummaries.Max(summary => summary?.CourseResult?.Count) ?? 1;
            ViewBag.MaxCourseResults = maxCourseResults;

            foreach (var StudentSummary in studentSummaries)
            {
                StudentSummary.bufferRows = maxCourseResults - (StudentSummary?.CourseResult?.Count ?? 0);
            }

            return studentSummaries;
        }
    }
}