using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Work.Models;
using Work.Util;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Work.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly YoutubeClient _youtube;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _youtube = new YoutubeClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> BaixarVideo(string videoUrl, CaminhoDownload request)
        {
            try
            {
                var videoInformacao = await _youtube.Videos.GetAsync(videoUrl);
                var videoTitulo = videoInformacao.Title;
                var diretorioDeSaida = "/home/gabriel/Desktop/videos";

                videoTitulo = FormNome.FormatarNomeArquivo(videoTitulo, 100);
                var diretorioSaida = Path.Combine(diretorioDeSaida, $"{videoTitulo}.mp4");

                Directory.CreateDirectory(diretorioDeSaida);

                var informacaoUrl = await _youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var infoFLuxo = informacaoUrl.GetMuxedStreams().GetWithHighestVideoQuality();

                if (infoFLuxo != null)
                {
                    await _youtube.Videos.Streams.DownloadAsync(infoFLuxo, diretorioSaida);
                }
                else
                {
                    throw new Exception("Nenhuma stream de vídeo disponível.");
                }

                return PhysicalFile(diretorioSaida, "video/mp4", Path.GetFileName(diretorioSaida));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BaixarAudio(string videoUrl, CaminhoDownload request)
        {
            try
            {
                var videoInformacao = await _youtube.Videos.GetAsync(videoUrl);
                var videoTitulo = videoInformacao.Title;
                var diretorioDeSaida = "/home/gabriel/Desktop/videos";

                videoTitulo = FormNome.FormatarNomeArquivo(videoTitulo, 100);
                var diretorioSaida = Path.Combine(diretorioDeSaida, $"{videoTitulo}.mp3");

                Directory.CreateDirectory(diretorioDeSaida);

                var informacaoUrl = await _youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var infoFluxoAudio = informacaoUrl.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (infoFluxoAudio != null)
                {
                    await _youtube.Videos.Streams.DownloadAsync(infoFluxoAudio, diretorioSaida);
                }
                else
                {
                    throw new Exception("Nenhuma stream de áudio disponível.");
                }

                return PhysicalFile(diretorioSaida, "audio/mpeg", Path.GetFileName(diretorioSaida));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
