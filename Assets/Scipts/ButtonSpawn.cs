using DefaultNamespace;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSpawn : MonoBehaviour
{
    public EntityType entityType;
    public SpawnLocatorService _locator;
    public Button _buttonSpawner;
    public GameObject _entity;


    void Start()
    {
        _buttonSpawner.onClick.AddListener(SpawnEntity);
    }


    private void SpawnEntity()
    {
        var newEntity = GameObject.Instantiate(_entity);
        newEntity.transform.localPosition = _locator.FindLocalPosition(entityType);
    }
}