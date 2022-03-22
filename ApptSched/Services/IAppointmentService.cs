using System.Collections.Generic;
using System.Threading.Tasks;
using ApptSched.Models.ViewModels;
using Microsoft.Data.SqlClient.DataClassification;

namespace ApptSched.Services
{
    public interface IAppointmentService
    {
        public List<DoctorModel> GetDoctors();
        public List<PatientModel> GetPatients();
        public Task<int> AddUpdate(AppointmentVM model);

        public List<AppointmentVM> DoctorsEventsById(string doctorId);
        public List<AppointmentVM> PatientsEventsById(string patientId);
        public AppointmentVM GetById(int id);
        public Task<int> DeleteAppointment(int id);
        public Task<int> ConfirmEvent(int id);
    }
}
