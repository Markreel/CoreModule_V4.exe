using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;

public class UntitledScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async void GetHttpsAsync()
    {
        using (var _client = new HttpClient())
        {
            var _result = await _client.GetAsync("INSERT URL");
            if (_result.IsSuccessStatusCode)
            {
                Debug.Log(await _result.Content.ReadAsStringAsync());
            }
        }
    }
}
