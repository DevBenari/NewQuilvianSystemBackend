# Farmasi — State Transition Routing Depo

Contract version: `PHA-STATE-ROUTING-v1`; status `approved`; input `PHA-DA-001-r1`; disetujui product/domain owner 21 Agustus 2026.

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `NotEvaluated` | Resolve | `Resolved` | Sistem | Tepat satu kandidat | Proses berhenti |
| `NotEvaluated` | Resolve | `RejectedNoCandidate` | Sistem | Nol kandidat | Tampilkan kesalahan konfigurasi |
| `NotEvaluated` | Resolve | `RejectedAmbiguous` | Sistem | Lebih dari satu kandidat | Tampilkan kesalahan konfigurasi |
| `Resolved` | Revalidate | `Resolved` baru | Sistem | Kandidat tetap tepat satu | Histori evaluasi lama tidak diubah |
| `Resolved` | Revalidate | `Rejected*` | Sistem | Konfigurasi berubah | Jangan memindahkan Depo diam-diam |

Transisi langsung dari rejection ke reservasi atau dispense tidak sah.
