using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SkillSwap.Pages
{
    public class ImagesModel : PageModel
    {
        private readonly ILogger<ImagesModel> _logger;

        public ImagesModel(ILogger<ImagesModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGetHeroIllustration()
        {
            string svgContent = @"<svg width=""800"" height=""600"" viewBox=""0 0 800 600"" xmlns=""http://www.w3.org/2000/svg"">
              <defs>
                <filter id=""shadow"" x=""-50%"" y=""-50%"" width=""200%"" height=""200%"">
                  <feGaussianBlur stdDeviation=""10"" />
                </filter>
              </defs>
              
              <!-- Background shadow -->
              <ellipse cx=""400"" cy=""480"" rx=""300"" ry=""40"" fill=""#e0e0e0"" filter=""url(#shadow)"" opacity=""0.3"" />
              
              <!-- Left person -->
              <g transform=""translate(250, 300)"">
                <!-- Body -->
                <ellipse cx=""0"" cy=""0"" rx=""60"" ry=""100"" fill=""#4285f4"" />
                
                <!-- Head -->
                <circle cx=""0"" cy=""-130"" r=""50"" fill=""#ffddc1"" />
                
                <!-- Eyes -->
                <circle cx=""-15"" cy=""-135"" r=""5"" fill=""#333"" />
                <circle cx=""15"" cy=""-135"" r=""5"" fill=""#333"" />
                
                <!-- Smile -->
                <path d=""M-20,-115 Q0,-95 20,-115"" stroke=""#333"" stroke-width=""3"" fill=""none"" />
                
                <!-- Arm holding skill -->
                <path d=""M40,-50 Q80,-30 100,20"" stroke=""#4285f4"" stroke-width=""20"" stroke-linecap=""round"" fill=""none"" />
                
                <!-- Skill bubble -->
                <circle cx=""120"" cy=""30"" r=""50"" fill=""#34a853"" />
                <text x=""120"" y=""35"" font-family=""Arial"" font-size=""16"" fill=""white"" text-anchor=""middle"">Skills to Share</text>
              </g>
              
              <!-- Right person -->
              <g transform=""translate(550, 300)"">
                <!-- Body -->
                <ellipse cx=""0"" cy=""0"" rx=""60"" ry=""100"" fill=""#fbbc05"" />
                
                <!-- Head -->
                <circle cx=""0"" cy=""-130"" r=""50"" fill=""#f8cba6"" />
                
                <!-- Eyes -->
                <circle cx=""-15"" cy=""-135"" r=""5"" fill=""#333"" />
                <circle cx=""15"" cy=""-135"" r=""5"" fill=""#333"" />
                
                <!-- Smile -->
                <path d=""M-20,-115 Q0,-95 20,-115"" stroke=""#333"" stroke-width=""3"" fill=""none"" />
                
                <!-- Arm reaching for skill -->
                <path d=""M-40,-50 Q-80,-30 -100,20"" stroke=""#fbbc05"" stroke-width=""20"" stroke-linecap=""round"" fill=""none"" />
                
                <!-- Skill need bubble -->
                <circle cx=""-120"" cy=""30"" r=""50"" fill=""#4285f4"" />
                <text x=""-120"" y=""35"" font-family=""Arial"" font-size=""16"" fill=""white"" text-anchor=""middle"">Skills to Learn</text>
              </g>
              
              <!-- Exchange arrows -->
              <g transform=""translate(400, 300)"">
                <path d=""M-70,-50 L70,-50"" stroke=""#ea4335"" stroke-width=""8"" stroke-linecap=""round"" fill=""none"" />
                <path d=""M60,-50 L40,-30"" stroke=""#ea4335"" stroke-width=""8"" stroke-linecap=""round"" fill=""none"" />
                <path d=""M60,-50 L40,-70"" stroke=""#ea4335"" stroke-width=""8"" stroke-linecap=""round"" fill=""none"" />
                
                <path d=""M70,50 L-70,50"" stroke=""#ea4335"" stroke-width=""8"" stroke-linecap=""round"" fill=""none"" />
                <path d=""M-60,50 L-40,30"" stroke=""#ea4335"" stroke-width=""8"" stroke-linecap=""round"" fill=""none"" />
                <path d=""M-60,50 L-40,70"" stroke=""#ea4335"" stroke-width=""8"" stroke-linecap=""round"" fill=""none"" />
              </g>
            </svg>";

            return Content(svgContent, "image/svg+xml");
        }

        public IActionResult OnGetAboutImage()
        {
            string svgContent = @"<svg width=""800"" height=""600"" viewBox=""0 0 800 600"" xmlns=""http://www.w3.org/2000/svg"">
              <!-- Background -->
              <rect x=""0"" y=""0"" width=""800"" height=""600"" fill=""#f8f9fa"" rx=""20"" ry=""20"" />
              
              <!-- SkillSwap Logo -->
              <g transform=""translate(400, 150)"">
                <circle cx=""0"" cy=""0"" r=""100"" fill=""#4285f4"" />
                <text x=""0"" y=""15"" font-family=""Arial"" font-size=""40"" font-weight=""bold"" fill=""white"" text-anchor=""middle"">SkillSwap</text>
              </g>
              
              <!-- People around the logo -->
              <g transform=""translate(200, 300)"">
                <!-- First person -->
                <circle cx=""0"" cy=""0"" r=""40"" fill=""#34a853"" />
                <circle cx=""0"" cy=""-25"" r=""20"" fill=""#ffddc1"" />
                
                <!-- Skill icon -->
                <circle cx=""-70"" cy=""-30"" r=""25"" fill=""#fbbc05"" />
                <text x=""-70"" y=""-25"" font-family=""Arial"" font-size=""10"" fill=""white"" text-anchor=""middle"">Coding</text>
                
                <!-- Connection line -->
                <path d=""M-20,-15 L-45,-25"" stroke=""#333"" stroke-width=""2"" />
              </g>
              
              <g transform=""translate(600, 300)"">
                <!-- Second person -->
                <circle cx=""0"" cy=""0"" r=""40"" fill=""#ea4335"" />
                <circle cx=""0"" cy=""-25"" r=""20"" fill=""#f8cba6"" />
                
                <!-- Skill icon -->
                <circle cx=""70"" cy=""-30"" r=""25"" fill=""#4285f4"" />
                <text x=""70"" y=""-25"" font-family=""Arial"" font-size=""10"" fill=""white"" text-anchor=""middle"">Music</text>
                
                <!-- Connection line -->
                <path d=""M20,-15 L45,-25"" stroke=""#333"" stroke-width=""2"" />
              </g>
              
              <g transform=""translate(300, 450)"">
                <!-- Third person -->
                <circle cx=""0"" cy=""0"" r=""40"" fill=""#fbbc05"" />
                <circle cx=""0"" cy=""-25"" r=""20"" fill=""#ffddc1"" />
                
                <!-- Skill icon -->
                <circle cx=""-50"" cy=""50"" r=""25"" fill=""#34a853"" />
                <text x=""-50"" y=""55"" font-family=""Arial"" font-size=""10"" fill=""white"" text-anchor=""middle"">Yoga</text>
                
                <!-- Connection line -->
                <path d=""M-15,15 L-35,35"" stroke=""#333"" stroke-width=""2"" />
              </g>
              
              <g transform=""translate(500, 450)"">
                <!-- Fourth person -->
                <circle cx=""0"" cy=""0"" r=""40"" fill=""#4285f4"" />
                <circle cx=""0"" cy=""-25"" r=""20"" fill=""#f8cba6"" />
                
                <!-- Skill icon -->
                <circle cx=""50"" cy=""50"" r=""25"" fill=""#ea4335"" />
                <text x=""50"" y=""55"" font-family=""Arial"" font-size=""10"" fill=""white"" text-anchor=""middle"">Cook</text>
                
                <!-- Connection line -->
                <path d=""M15,15 L35,35"" stroke=""#333"" stroke-width=""2"" />
              </g>
              
              <!-- Connection lines between people -->
              <path d=""M230,300 L570,300"" stroke=""#333"" stroke-width=""2"" stroke-dasharray=""5,5"" />
              <path d=""M300,450 L500,450"" stroke=""#333"" stroke-width=""2"" stroke-dasharray=""5,5"" />
              <path d=""M230,300 L300,450"" stroke=""#333"" stroke-width=""2"" stroke-dasharray=""5,5"" />
              <path d=""M570,300 L500,450"" stroke=""#333"" stroke-width=""2"" stroke-dasharray=""5,5"" />
              
              <!-- Text -->
              <text x=""400"" y=""550"" font-family=""Arial"" font-size=""20"" font-weight=""bold"" fill=""#333"" text-anchor=""middle"">
                Connecting skills, expanding horizons
              </text>
            </svg>";

            return Content(svgContent, "image/svg+xml");
        }
    }
}