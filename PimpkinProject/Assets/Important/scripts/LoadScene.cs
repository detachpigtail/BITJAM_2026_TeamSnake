using UnityEngine;
using UnityEngine.SceneManagement;

namespace AA0000
{
    public class LoadScene : MonoBehaviour
    {
        public string _sceneToLoad;
		public bool _loadAdditively = true;

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				if (_loadAdditively)
				{
					SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
				}
				else
				{ 
					SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Single);
				}
				gameObject.SetActive(false);
			}
		}
	} 
}
