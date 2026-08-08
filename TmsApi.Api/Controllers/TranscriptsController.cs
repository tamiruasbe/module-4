// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.RateLimiting;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/v2/transcripts")]
// public class TranscriptsController : ControllerBase
// {
//     // [HttpPost]
//     // [EnableRateLimiting("transcripts")]
//     // public IActionResult RequestTranscript([FromBody] object? _)
//     // {
//     //     // Stub: Exercise 5 replaces this with enqueue + 202 + Location
//     //     return Ok();
//     // }

//     [HttpPost]
// [EnableRateLimiting("transcripts")]
// public async Task<IActionResult> RequestTranscript(
//     [FromBody] object? _,
//     CancellationToken ct)
// {
//     // Simulate transcript generation
//     await Task.Delay(TimeSpan.FromSeconds(5), ct);

//     return Ok();
// }
// }