using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatRoom.Models.DTOs;

namespace ChatRoom.MVC.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ChatRoomAPI");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var json = JsonSerializer.Serialize(loginDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<AuthResponseDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            var json = JsonSerializer.Serialize(registerDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<AuthResponseDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<List<UserDto>>> GetUsersAsync(string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync("/api/users");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<List<UserDto>>>(responseContent, _jsonOptions)
                ?? new ApiResponse<List<UserDto>> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId, string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync($"/api/users/{userId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string token)
        {
            SetAuthorizationHeader(token);
            var json = JsonSerializer.Serialize(createUserDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/users", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto, string token)
        {
            SetAuthorizationHeader(token);
            var json = JsonSerializer.Serialize(updateUserDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/users/{userId}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse> DeleteUserAsync(string userId, string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.DeleteAsync($"/api/users/{userId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions)
                ?? new ApiResponse { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse> AssignRoleAsync(AssignRoleDto assignRoleDto, string token)
        {
            SetAuthorizationHeader(token);
            var json = JsonSerializer.Serialize(assignRoleDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/users/assign-role", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions)
                ?? new ApiResponse { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<List<string>>> GetRolesAsync(string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync("/api/users/roles");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<List<string>>>(responseContent, _jsonOptions)
                ?? new ApiResponse<List<string>> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<List<TaskDto>>> GetTasksAsync(string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync("/api/tasks");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<List<TaskDto>>>(responseContent, _jsonOptions)
                ?? new ApiResponse<List<TaskDto>> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int taskId, string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync($"/api/tasks/{taskId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<TaskDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<TaskDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto, string token)
        {
            SetAuthorizationHeader(token);
            var json = JsonSerializer.Serialize(createTaskDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/tasks", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<TaskDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<TaskDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, string token)
        {
            SetAuthorizationHeader(token);
            var json = JsonSerializer.Serialize(updateTaskDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/tasks/{taskId}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<TaskDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<TaskDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse> DeleteTaskAsync(int taskId, string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.DeleteAsync($"/api/tasks/{taskId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions)
                ?? new ApiResponse { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<TaskDto>> ToggleTaskCompletionAsync(int taskId, string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.PutAsync($"/api/tasks/{taskId}/toggle", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<TaskDto>>(responseContent, _jsonOptions)
                ?? new ApiResponse<TaskDto> { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<ApiResponse<List<TaskDto>>> GetTasksByUserAsync(string userId, string token)
        {
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync($"/api/tasks/user/{userId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<List<TaskDto>>>(responseContent, _jsonOptions)
                ?? new ApiResponse<List<TaskDto>> { Success = false, Message = "Failed to deserialize response" };
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}