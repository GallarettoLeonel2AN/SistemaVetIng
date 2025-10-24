using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

[Authorize(Roles = "Veterinaria")]
public class VacunaController : Controller
{
    private readonly IVacunaService _vacunaService;
    private readonly IToastNotification _toastNotification;

    public VacunaController(IVacunaService vacunaService, IToastNotification toastNotification)
    {
        _vacunaService = vacunaService;
        _toastNotification = toastNotification;
    }

   
    [HttpGet]
    public async Task<IActionResult> Index(int? page, string busqueda)
    {
        int pageNumber = page ?? 1;
        int pageSize = 10;

        var vacunasPaginadas = await _vacunaService.ListarPaginadoAsync(pageNumber, pageSize, busqueda);

        ViewBag.Busqueda = busqueda;
        ViewBag.PageNumber = pageNumber;
        return View(vacunasPaginadas);
    }

    [HttpGet]
    public IActionResult Crear() => View(new VacunaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(VacunaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _toastNotification.AddErrorToastMessage("Revise los datos del formulario.");
            return View(model);
        }

        var (success, message) = await _vacunaService.Registrar(model);

        if (success)
        {
            _toastNotification.AddSuccessToastMessage(message);
            return RedirectToAction(nameof(Index));
        }
        else
        {
            _toastNotification.AddErrorToastMessage(message);
            return View(model);
        }
    }

   
    [HttpGet]
    public async Task<IActionResult> Modificar(int id)
    {
        var vacuna = await _vacunaService.ObtenerPorId(id);
        if (vacuna == null)
        {
            _toastNotification.AddErrorToastMessage("Vacuna no encontrada.");
            return RedirectToAction(nameof(Index));
        }
        return View(vacuna);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modificar(VacunaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _toastNotification.AddErrorToastMessage("Revise los datos del formulario.");
            return View(model);
        }

        var (success, message) = await _vacunaService.Modificar(model);

        if (success)
        {
            _toastNotification.AddSuccessToastMessage(message);
            return RedirectToAction(nameof(Index));
        }
        else
        {
            _toastNotification.AddErrorToastMessage(message);
            return View(model);
        }
    }

    
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var (success, message) = await _vacunaService.Eliminar(id);

        if (success)
        {
            _toastNotification.AddSuccessToastMessage(message);
        }
        else
        {
            _toastNotification.AddErrorToastMessage(message);
        }

        return RedirectToAction(nameof(Index));
    }
}