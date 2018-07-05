using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace BSA_Data_Structures_and_LINQ_Dushkevych
{
    enum DataType { Users, Posts, Comments, Todos };

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Retrieving all data. Please be patient...");
            string[] dataSources = new string[] {   "https://5b128555d50a5c0014ef1204.mockapi.io/users",
                                                    "https://5b128555d50a5c0014ef1204.mockapi.io/posts",
                                                    "https://5b128555d50a5c0014ef1204.mockapi.io/comments",
                                                    "https://5b128555d50a5c0014ef1204.mockapi.io/todos" };
            List<User> users = GetUsers(dataSources[(int)DataType.Users]);
            List<Post> posts = GetPosts(dataSources[(int)DataType.Posts]);
            List<Comment> comments = GetComments(dataSources[(int)DataType.Comments]);
            List<Todo> todos = GetTodos(dataSources[(int)DataType.Todos]);

            IEnumerable<User> allData = from user in users
                          select new User()
                          {
                              Id = user.Id,
                              CreatedAt = user.CreatedAt,
                              Name = user.Name,
                              Avatar = user.Avatar,
                              Email = user.Email,
                              Posts = (from post in posts
                                       where user.Id == post.UserId
                                       select new Post()
                                       {
                                           Id = post.Id,
                                           CreatedAt = post.CreatedAt,
                                           Title = post.Title,
                                           Body = post.Body,
                                           UserId = post.UserId,
                                           Likes = post.Likes,
                                           Comments = (from comment in comments
                                                       where post.Id == comment.PostId
                                                       select new Comment()
                                                       {
                                                           Id = comment.Id,
                                                           CreatedAt = comment.CreatedAt,
                                                           Body = comment.Body,
                                                           UserId = comment.UserId,
                                                           PostId = comment.PostId,
                                                           Likes = comment.Likes,
                                                       }).ToList()
                                       }).ToList(),
                              Todos = (from todo in todos
                                       where user.Id == todo.UserId
                                       select new Todo()
                                       {
                                           Id = todo.Id,
                                           CreatedAt = todo.CreatedAt,
                                           Name = todo.Name,
                                           IsComplete = todo.IsComplete,
                                           UserId = todo.UserId,
                                       }).ToList()
                          };
            Console.WriteLine($"Got all the data! Select what you want to do:\n1. Get number of comments under user's posts by UserId\n2. Get user's comments(<50 symbols) list by UserId\n3. Get user's done Todo list tasks by UserId\n4. Get sorted users list with their sorted todo tasks\n5. Get specific user's info\n6. Get specific posts info");
            int choise = Convert.ToInt32(Console.ReadLine());
            if (1 > choise || choise > 6)
            {
                Console.WriteLine("Wrong choice! Start over.");
            }
            else
            {
                int userId;
                switch (choise)
                {                    
                    case 1:
                        Console.WriteLine("Enter UserId(1-100)");
                        userId = Convert.ToInt32(Console.ReadLine());
                        PrintPostsNumberByUserId(allData, userId);
                        break;
                    case 2:
                        Console.WriteLine("Enter UserId(1-100)");
                        userId = Convert.ToInt32(Console.ReadLine());
                        PrintPostCommentsListByUserId(allData, userId);
                        break;
                    case 3:
                        Console.WriteLine("Enter UserId(1-100)");
                        userId = Convert.ToInt32(Console.ReadLine());
                        PrintDoneTodosByUserId(allData, userId);
                        break;
                    case 4:
                        PrintUsersAscTodosDesc(allData);
                        break;
                    case 5:
                        Console.WriteLine("Enter UserId(1-100)");
                        userId = Convert.ToInt32(Console.ReadLine());
                        PrintSpecificUserInfoByUserId(allData, userId);
                        break;
                    case 6:
                        Console.WriteLine("Enter PostId(1-100)");
                        int postId = Convert.ToInt32(Console.ReadLine());
                        PrintSpecificPostInfoByPostId(allData, postId);
                        break;
                    default:
                        break;
                }
            }
            Console.ReadKey();
        }

        //1
        static void PrintPostsNumberByUserId(IEnumerable<User> allData, int userId)
        {
            var selectResult = allData.Where(x => x.Id == userId)
                                      .SelectMany(x => x.Posts)
                                      .Select(y => new { y.Title, y.Comments.Count})
                                      .ToList();

            foreach (var item in selectResult)
            {
                Console.WriteLine($"{item.Title} | {item.Count}");
            }
        }

        //2
        static void PrintPostCommentsListByUserId(IEnumerable<User> allData, int userId)
        {
            var selectResult = allData.Where(u => u.Id == userId)
                                      .SelectMany(u => u.Posts)
                                      .SelectMany(p => p.Comments)
                                      .Where(c => c.Body.Length < 50).ToList();
            if(selectResult.Count == 0)
            {
                Console.WriteLine("Nothing to show!");
            }
            else
            {
                foreach (var item in selectResult)
                {
                    Console.WriteLine($"{item.Body}");
                }
            }            
        }

        //3
        static void PrintDoneTodosByUserId(IEnumerable<User> allData, int userId)
        {
            var selectResult = allData.Where(u => u.Id == userId)
                                        .SelectMany(u => u.Todos)
                                        .Where(t => t.IsComplete)
                                        .Select(t => new { t.Id, t.Name})
                                        .ToList();
            if (selectResult.Count == 0)
            {
                Console.WriteLine("Nothing to show!");
            }
            else
            {
                foreach (var item in selectResult)
                {
                    Console.WriteLine($"{item.Id} | {item.Name}");
                }
            }
        }

        //4
        static void PrintUsersAscTodosDesc(IEnumerable<User> allData)
        {
            var selectResult = allData.OrderBy(x => x.Name).ToList();
            foreach (var user in selectResult)
            {
                user.Todos = user.Todos.OrderByDescending(x => x.Name.Length).ToList();
            }
            foreach (var item in selectResult)
            {                
                foreach (var todo in item.Todos)
                {
                    Console.WriteLine($"{item.Name} | {todo.Name}");
                }
            }
            
        }

        //5
        static void PrintSpecificUserInfoByUserId(IEnumerable<User> allData, int userId)
        {
            var user = allData.Where(u => u.Id == userId).First();
            var lastPost = user.Posts.OrderByDescending(p => p.CreatedAt).First();
            var commentsOnLastPost = lastPost.Comments.Count;
            var undoneTasks = user.Todos.Count(t => t.IsComplete == false);
            var popularBigPost = user.Posts.OrderByDescending(x => x.Comments.Count(c => c.Body.Length > 80)).First();
            var mostPopularPostByLikes = user.Posts.OrderByDescending(p => p.Likes).First();
            Console.WriteLine($"User: {user.Name}");
            Console.WriteLine($"Last post: {lastPost.Body}");
            Console.WriteLine($"Comments on last post: {commentsOnLastPost}");
            Console.WriteLine($"Undone tasks: {undoneTasks}");
            Console.WriteLine($"Most popular big post: {popularBigPost.Body}");
            Console.WriteLine($"Most liked post: {mostPopularPostByLikes.Body}");
        }

        //6
        static void PrintSpecificPostInfoByPostId(IEnumerable<User> allData, int postId)
        {
            var post = allData.SelectMany(u => u.Posts).Where(p => p.Id == postId).First();
            var longestComment = post.Comments.Select(c => c).OrderByDescending(c => c.Body.Length).First();
            var mostLikedComment = post.Comments.Select(c => c).OrderByDescending(c => c.Likes).First();
            var leastPopularCommentsCount = post.Comments.Where(c => c.Likes == 0 || c.Body.Length < 80).Count();

            
            Console.WriteLine($"Post: {post.Body}");
            Console.WriteLine($"Longest comment on post: {longestComment.Body}");
            Console.WriteLine($"Most liked comment: {mostLikedComment.Body}");
            Console.WriteLine($"Unpopular comments count: {leastPopularCommentsCount}");
        }

        static List<User> GetUsers (string url)
        {
            List<User> users;
            HttpResponseMessage response = GetHttpResponseMessage(url);
            users = JsonConvert.DeserializeObject<List<User>>(response.Content.ReadAsStringAsync().Result);
            return users;
        }

        static List<Post> GetPosts(string url)
        {
            List<Post> posts;
            HttpResponseMessage response = GetHttpResponseMessage(url);
            posts = JsonConvert.DeserializeObject<List<Post>>(response.Content.ReadAsStringAsync().Result);
            return posts;
        }

        static List<Comment> GetComments(string url)
        {
            List<Comment> comments;
            HttpResponseMessage response = GetHttpResponseMessage(url);
            comments = JsonConvert.DeserializeObject<List<Comment>>(response.Content.ReadAsStringAsync().Result);
            return comments;
        }

        static List<Todo> GetTodos(string url)
        {
            List<Todo> todos;
            HttpResponseMessage response = GetHttpResponseMessage(url);
            todos = JsonConvert.DeserializeObject<List<Todo>>(response.Content.ReadAsStringAsync().Result);
            return todos;
        }

        static HttpResponseMessage GetHttpResponseMessage(string url)
        {
            HttpResponseMessage response;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = client.GetAsync(url).Result;
            }
            return response;
        }
    }
}
