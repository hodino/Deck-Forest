using UnityEngine;

// 이 스크립트를 전투 씬이나 다른 씬의 '버튼'에 직접 달아줍니다.
public class UIOpener : MonoBehaviour
{
    // 옵션 버튼을 눌렀을 때 실행될 함수
    public void OpenOptions()
    {
        // 씬 어디엔가 살아있는 OptionsManager를 찾아서 열기 함수를 실행!
        // (OptionsManager에 OpenOptions() 같은 함수가 있다고 가정합니다)
        OptionsManager options = FindObjectOfType<OptionsManager>();
        if (options != null)
        {
            options.OpenOptions(); // 옵션 매니저 안의 창 열기 함수 이름에 맞게 수정하세요.
        }
    }

    // 인벤토리 버튼을 눌렀을 때 실행될 함수
    public void OpenInventory()
    {
        // 씬 어디엔가 살아있는 InventoryManager를 찾아서 열기 함수를 실행!
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null)
        {
            inventory.OpenInventory(); // 인벤토리 매니저 안의 창 열기 함수 이름
        }
    }
}