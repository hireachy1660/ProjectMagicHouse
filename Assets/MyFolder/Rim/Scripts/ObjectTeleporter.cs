using UnityEngine;

public class ObjectTeleporter : MonoBehaviour
{
    public Transform miniatureSpawnPoint; // �̴Ͼ�ó �� ������ ���� ���� (�� ������Ʈ)
    public float scaleRatio = 0.1f;       // �̴Ͼ�ó ����

    private void OnTriggerEnter(Collider other)
    {
        // 1. ���� ������ "Grabbable" Ȥ�� Ư�� �±����� Ȯ��
        if (other.CompareTag("Grabbable"))
        {
            // 2. ���� ���� ��������
            Rigidbody rb = other.GetComponent<Rigidbody>();

            // 3. ��ġ �̵�: ���� õ�忡 ���� ������ �̴Ͼ�ó �Ա��� �����̵�
            other.transform.position = miniatureSpawnPoint.position;

            // 4. ũ�� ���: �̴Ͼ�ó ������ �°� �۰� ����
            other.transform.localScale *= scaleRatio;

            // 5. �ӵ� ����: ������ ��ȭ���� ���ֱ� ���� �ӵ��� ������ŭ ����
            if (rb != null)
            {
                rb.linearVelocity *= scaleRatio;
            }
        }
    }
}