using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SiteContagem.Models;

namespace SiteContagem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly Contador _CONTADOR = new Contador();
        public static bool _SIMULAR_FALHA = false;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(
            [FromServices]IConfiguration configuration)
        {
            lock (_CONTADOR)
            {
                _CONTADOR.Incrementar();
                _logger.LogInformation($"Contador - Valor atual: {_CONTADOR.ValorAtual}");

                TempData["Contador"] = _CONTADOR.ValorAtual;
                TempData["Local"] = _CONTADOR.Local;
                TempData["Kernel"] = _CONTADOR.Kernel;
                TempData["TargetFramework"] = _CONTADOR.TargetFramework;
                TempData["MensagemFixa"] = "Teste";
                TempData["MensagemVariavel"] = configuration["MensagemVariavel"];
                TempData["SimulacaoFalha"] = _SIMULAR_FALHA ? "Sim" : "Não";
            }            

            return View();
        }

        [HttpGet("status")]
        public IActionResult Get()
        {
            if (!_SIMULAR_FALHA)
            {
                _logger.LogInformation("Simulação de falha desativada.");
                return new OkObjectResult(new { SimularFalha = _SIMULAR_FALHA, Mensagem = "OK" });
            }

            _logger.LogError("Simulação de falha ativada.");
            throw new Exception("Simulação falhas via Health Check.");
        }

        [HttpGet("statuserro")]
        public IActionResult GetStatusErro()
        {
            _logger.LogInformation("Forçando a simulação de falha via Health Check.");
            _SIMULAR_FALHA = true;
            return new OkObjectResult(new
            {
                Local = _CONTADOR.Local,
                Mensagem = "Simulação de falha via Health Check ativada."
            });
        }

        [HttpGet("statusnormal")]
        public IActionResult GetStatusNormal()
        {
            _logger.LogInformation("Desativando a simulação de falha via Health Check.");
            _SIMULAR_FALHA = false;
            return new OkObjectResult(new
            {
                Local = _CONTADOR.Local,
                Mensagem = "Simulação de falha via Health Check desativada."
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}