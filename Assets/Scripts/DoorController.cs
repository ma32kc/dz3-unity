using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform interactor;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
    [SerializeField] private bool requireTriggerZone = true;
    [SerializeField, Min(0f)] private float interactionCooldown = 1f;

    [Header("Trigger Zones")]
    [SerializeField] private Collider[] triggerZones;

    [Header("Animator")]
    [SerializeField] private string openTriggerName = "DoorTrigger";

    private Animator _animator;
    private float _nextInteractTime;
    private bool _interactorInTrigger;
    private int _triggerOccupants;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (interactor == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                interactor = player.transform;
            }
        }

        if (triggerZones == null || triggerZones.Length == 0)
        {
            CacheTriggerZonesFromChildren();
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(interactKey))
        {
            return;
        }

        if (!CanInteract())
        {
            return;
        }

        _animator.SetTrigger(openTriggerName);
        _nextInteractTime = Time.time + interactionCooldown;
    }

    private bool CanInteract()
    {
        if (Time.time < _nextInteractTime)
        {
            return false;
        }

        if (interactor == null)
        {
            return false;
        }

        float distance = Vector3.Distance(interactor.position, transform.position);
        if (distance > interactionDistance)
        {
            return false;
        }

        if (requireTriggerZone && HasUsableTriggerZones() && !IsInteractorInsideTriggerZone())
        {
            return false;
        }

        return true;
    }

    private bool HasUsableTriggerZones()
    {
        if (triggerZones == null || triggerZones.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < triggerZones.Length; i++)
        {
            Collider zone = triggerZones[i];
            if (zone != null && zone.enabled && zone.isTrigger && zone.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInteractorInsideTriggerZone()
    {
        if (_interactorInTrigger)
        {
            return true;
        }

        if (interactor == null || triggerZones == null)
        {
            return false;
        }

        Vector3 point = interactor.position;
        for (int i = 0; i < triggerZones.Length; i++)
        {
            Collider zone = triggerZones[i];
            if (zone == null || !zone.enabled || !zone.isTrigger || !zone.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 closest = zone.ClosestPoint(point);
            if ((closest - point).sqrMagnitude < 0.0001f)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInteractorCollider(other))
        {
            return;
        }

        _triggerOccupants++;
        _interactorInTrigger = _triggerOccupants > 0;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInteractorCollider(other))
        {
            return;
        }

        _triggerOccupants = Mathf.Max(0, _triggerOccupants - 1);
        _interactorInTrigger = _triggerOccupants > 0;
    }

    private bool IsInteractorCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (interactor != null)
        {
            return other.transform == interactor || other.transform.IsChildOf(interactor);
        }

        return other.CompareTag("Player");
    }

    private void CacheTriggerZonesFromChildren()
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);
        List<Collider> triggers = new List<Collider>(allColliders.Length);

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col != null && col.isTrigger)
            {
                triggers.Add(col);
            }
        }

        triggerZones = triggers.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
