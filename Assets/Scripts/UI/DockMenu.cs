using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class DockMenu : MonoBehaviour
{
    [SerializeField] private Button _restockButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private int _priceForUpgrade = 2;
    [SerializeField] private int _priceIncrease = 2;
    private bool _menuIsOpen = false;
    private PlayerOnShipInputHandler _playerInputHandler;
    private int _score;

    public void CloseDockMenu()
    {
        Dock.PlayerInDock = false;
        _menuIsOpen = false;
        foreach (Transform child in transform)
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        _playerInputHandler.enabled = true;

        _restockButton.onClick.RemoveListener(RestockAction);

        CursorController.SetCursorOff();
    }

    public void UpgradeShip()
    {
        GameObject player = _playerInputHandler.gameObject;
        player.GetComponent<ShipManager>().UpgradeShip();
        _priceForUpgrade += _priceIncrease;
    }

    public void RestockAction()
    {
        GameObject playerShip = GameObject.FindGameObjectWithTag("PlayerShip");
        playerShip.GetComponent<ShipController>().Restock();
    }

    public void UpdateButtonsInfo()
    {
        _menuIsOpen = true;

        _score = Convert.ToInt32(GameObject.FindGameObjectWithTag("ScoreCounter").GetComponent<TextMeshProUGUI>().text);
        _playerInputHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerOnShipInputHandler>();
        _playerInputHandler.enabled = false;
        UpdateUpgradeButtonInfo();
    }

    private void UpdateUpgradeButtonInfo()
    {
        TextMeshProUGUI upgradeButtonObj = _upgradeButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        if (ShipManager.FinalShip)
        {
            upgradeButtonObj.text = "You have the best ship";
            return;
        }

        if (_score - _priceForUpgrade >= 0)
        {
            _upgradeButton.enabled = true;
            upgradeButtonObj.text = "Upgrade ship, cost is: " + _priceForUpgrade;
        }
        else
        {
            _upgradeButton.enabled = false;
            upgradeButtonObj.text = "Not enough score to upgrade, price is: " + _priceForUpgrade;
        }
    }

    private void OpenMenu()
    {
        CursorController.SetCursorOn();
        Dock.PlayerInDock = true;
        UpdateButtonsInfo();

        foreach (Transform child in transform)
            child.gameObject.SetActive(!child.gameObject.activeSelf);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && _menuIsOpen == false)
            OpenMenu();
    }

    private void OnDisable()
    {
        _restockButton.onClick.RemoveAllListeners();
        _upgradeButton.onClick.RemoveAllListeners();
        _menuIsOpen = false;
    }
}
