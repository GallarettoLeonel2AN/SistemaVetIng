using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

[Authorize(Roles = "Veterinaria,Veterinario")]
public class EstudioController : Controller
{
    private readonly IEstudioService _estudioService;
    private readonly IToastNotification _toastNotification;

    public EstudioController(IEstudioService estudioService, IToastNotification toastNotification)
    {
        _estudioService = estudioService;
        _toastNotification = toastNotification;
    }

    
    [HttpGet]
    public async Task<IActionResult> Index(int? page, string busqueda)
    {
        int pageNumber = page ?? 1;
        int pageSize = 10;

        var estudiosPaginados = await _estudioService.ListarPaginadoAsync(pageNumber, pageSize, busqueda);

        ViewBag.Busqueda = busqueda;
        ViewBag.PageNumber = pageNumber;
        return View(estudiosPaginados);
    }


    #region CREAR ESTUDIO

    [HttpGet]
    public IActionResult Crear() => View(new EstudioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(EstudioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _toastNotification.AddErrorToastMessage("Revise los datos del formulario.");
            return View(model);
        }

        var (success, message) = await _estudioService.Registrar(model);

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

    #endregion

    #region MODIFICAR ESTUDIO
    [HttpGet]
    public async Task<IActionResult> Modificar(int id)
    {
        var estudio = await _estudioService.ObtenerPorId(id);
        if (estudio == null)
        {
            _toastNotification.AddErrorToastMessage("Estudio no encontrado.");
            return RedirectToAction(nameof(Index));
        }
        return View(estudio);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modificar(EstudioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _toastNotification.AddErrorToastMessage("Revise los datos del formulario.");
            return View(model);
        }

        var (success, message) = await _estudioService.Modificar(model);

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
    #endregion

    #region ELIMINAR ESTUDIO
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var (success, message) = await _estudioService.Eliminar(id);

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
    #endregion
}