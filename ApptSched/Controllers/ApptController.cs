using ApptSched.Services;
using ApptSched.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApptSched.Controllers
{
    [Authorize] //only authorized user can access action methods
    public class ApptController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public ApptController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;   
        }
        //Appointment Page
        public IActionResult Index()
        { 
            ViewBag.Duration = Helper.GetTimeDropDown(); 
            ViewBag.DoctorList = _appointmentService.GetDoctors();
            ViewBag.PatientList = _appointmentService.GetPatients();
            return View();
        }
    }
}
