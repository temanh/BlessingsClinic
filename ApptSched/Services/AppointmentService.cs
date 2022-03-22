using System;
using ApptSched.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using ApptSched.Models;
using ApptSched.Utilities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ApptSched.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AppointmentService(ApplicationDbContext db, IEmailSender emailSender) //dependency injection
        {
            _db=db;
            _emailSender = emailSender;
        }

        public async Task<int> AddUpdate(AppointmentVM model)
        {
            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.StartDate).AddMinutes(Convert.ToDouble(model.Duration));
            var patient = _db.Users.FirstOrDefault(u => u.Id == model.PatientId);
            var doctor = _db.Users.FirstOrDefault(u => u.Id == model.DoctorId);
            if (model != null && model.Id > 0)
            {
                //update
                var appointment = _db.Appointments.FirstOrDefault(x => x.Id == model.Id);
                appointment.Title = model.Title;
                appointment.Description = model.Description;
                appointment.StartDate = startDate;
                appointment.EndDate = endDate;
                appointment.Duration = model.Duration;
                appointment.DoctorId = model.DoctorId;
                appointment.PatientId = model.PatientId;
                appointment.isDoctorApproved = false;
                appointment.AdminId = model.AdminId;
                await _db.SaveChangesAsync();

                return 1; 
            }
            else
            {
                //create
                Appointment appointment = new Appointment()
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = model.Duration,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    isDoctorApproved = false,
                    AdminId = model.AdminId

                };
                await _emailSender.SendEmailAsync(doctor.Email, "Appointment Created",
                    $"Your appointment with {patient.Name} is created and in pending status. Please review appointment details.");
                await _emailSender.SendEmailAsync(patient.Email, "Appointment Created",
                    $"Your appointment with {doctor.Name} is created and in pending status. ");
                //pushing properties to database 
                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();
                return 2; //add

            }
        }

        public List<DoctorModel> GetDoctors()
        {
            var doctors = (from user in _db.Users   //extracting user from Asp.NetUsers, ignoring the prefix
                                            join userRoles in _db.UserRoles on user.Id equals userRoles.UserId 
                                            join roles in _db.Roles.Where(x=>x.Name==Helper.Doctor) on userRoles.RoleId equals roles.Id
                                            select new DoctorModel //known as projection, because we are NOT retrieving everything in Users just doctors
                                            {
                                                Id = user.Id,
                                                Name = user.Name

                                            }                         
                                            ).ToList();
                                            return doctors;          
        }

        public List<PatientModel> GetPatients()
        {
            var patients = (from user in _db.Users   //extracting user from Asp.NetUsers, ignoring the prefix
                    join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                    join roles in _db.Roles.Where(x => x.Name == Helper.Patient) on userRoles.RoleId equals roles.Id
                    select new PatientModel //known as projection, because we are NOT retrieving everything in Users just doctors
                    {
                        Id = user.Id,
                        Name = user.Name

                    }
                ).ToList();
            return patients;
        }


        public async Task<int> DeleteAppointment(int id)
        {
            var appointment = _db.Appointments.FirstOrDefault(x => x.Id == id);
            if (appointment != null)
            {
                _db.Appointments.Remove(appointment);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        List<AppointmentVM> IAppointmentService.DoctorsEventsById(string doctorId)
        {
            return _db.Appointments.Where(x => x.DoctorId == doctorId).ToList().Select(c => new AppointmentVM()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                isDoctorApproved = c.isDoctorApproved
            }).ToList();
        }

        AppointmentVM IAppointmentService.GetById(int id)
        {
            return _db.Appointments.Where(x => x.Id == id).ToList().Select(c => new AppointmentVM()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                isDoctorApproved = c.isDoctorApproved,
                PatientId = c.PatientId,
                DoctorId = c.DoctorId,
                PatientName = _db.Users.Where(x=>x.Id==c.PatientId).Select(x=>x.Name).FirstOrDefault(),
                DoctorName = _db.Users.Where(x => x.Id == c.DoctorId).Select(x => x.Name).FirstOrDefault(),
            }).SingleOrDefault();
        }

        List<AppointmentVM> IAppointmentService.PatientsEventsById(string patientId)
        {
            return _db.Appointments.Where(x => x.PatientId == patientId).ToList().Select(c => new AppointmentVM()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                isDoctorApproved = c.isDoctorApproved
            }).ToList();
        }

        async Task<int> IAppointmentService.ConfirmEvent(int id)
        {
            var appointment = _db.Appointments.FirstOrDefault(x => x.Id == id);
            if (appointment != null)
            {
                appointment.isDoctorApproved = true; 
                return await _db.SaveChangesAsync();
            }
            return 0;
        }
    }
}
