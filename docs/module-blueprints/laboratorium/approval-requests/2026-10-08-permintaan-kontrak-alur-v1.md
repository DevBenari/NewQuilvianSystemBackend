# Permintaan Persetujuan — Kontrak alur Lab dari kiosk sampai hasil mengikuti FE v1

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-019` |
| `tanggal` | 2026-10-08 |
| `pengaju` | `design-business-module`, menurunkan Amendment Pass putaran 26 |
| `rujukan` | `LAB-DEC-212`..`LAB-DEC-221` (BR-141), `LAB-FE-036`; decision log revision 91; `02-backend-architecture.md` bagian 26; `AC-302`..`AC-313`; [`LAB-EVD-013`](../evidence/2026-10-08-keputusan-paritas-alur-v1.md) |
| `status` | ✅ **`disetujui`** 2026-10-08 — Yoga Aji Pratama, pemilik modul: *"Setuju ketujuh butir"*; **ketujuh butir bagian 3 sesuai usulan** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen lima kontrak. Keputusan bisnisnya **sudah** diambil 2026-10-07/08; yang diminta adalah persetujuan **bentuk kontraknya**. **Kodenya sudah berjalan** — persetujuan membukukan, satu butir (6) meminta perbaikan |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Alur Lab baru (kiosk, Daftar Pasien OTC, Penerimaan satu Simpan, Terima Sampling dari daftar, kunci Lunas) sudah
dibangun dan diuji sungguhan, tetapi kontraknya belum ditulis. Usulan ini menuliskannya: dua daftar baca baru (untuk
kiosk dan untuk OTC), tiga ruas status bayar di daftar pasien, penunjuk prosedur pada rincian pesanan, nama pengambil
sampel pada data wadah, dan dua penolakan baru saat memproses pasien Tunai yang belum lunas. Tidak ada tabel, kolom,
maupun izin baru. Satu cacat ditemukan saat menulis kontrak: kiosk belum menghitung pesanan berstatus *Dikonfirmasi*,
sehingga pasiennya salah diarahkan ke pendaftaran.

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r42` | [37](../contracts/api-contract.md) | Dua endpoint baca baru; enam ruas respons; `orderedProcedures` diisi; dua `409` `start-process` |
| `LAB-PERM-v1` | revision 15 | [17](../contracts/permission-audit-matrix.md) | Dua endpoint dipetakan; nol aksi baru |
| `LAB-STATE-v1` | `r9` | [11](../contracts/state-transition-matrix.md) | Proses wajib lolos pembayaran |
| `LAB-VAL-v1` | `r18` | [20](../contracts/validation-matrix.md) | `VAL-152`, `VAL-153` |
| `LAB-INT-v1` | `r6` | [10](../contracts/integration-contract.md) | `INT-09` Billing (sementara), `INT-10` Registrasi |

## 3. Tujuh butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---|---|---|---|
| 1 | Bacaan pesanan untuk kiosk | **`GET /lab-orders/kiosk/pending-by-patient/{patientId}`** dijaga `KioskRead`, isi tanpa hasil/harga/keuangan | Penjaga lain berarti akun kiosk harus diberi izin modul Lab |
| 2 | Jendela waktu kiosk | **30 hari**, tetap (tidak dapat diubah pemanggil) | Angka lain tidak mengubah aturan; jendela tanpa batas membuat pesanan lama yang terlupa menahan pasien |
| 3 | Daftar OTC | **`GET /lab-patient-registrations/kiosk-encounters`**, `LabPatientRegistration : Read`, tab lewat `isReferral` | — |
| 4 | Ruas pembayaran | **`paymentStatus`/`isPaymentCleared`/`outstandingAmount`** per baris, satuan kunjungan | Satuan per pemeriksaan tidak didukung Billing (pembayaran dicatat per invoice) |
| 5 | Penolakan Proses | **`VAL-152`** belum lunas dan **`VAL-153`** belum ditagih, keduanya `409` sesudah `VAL-151` | — |
| 6 | **Perbaikan cacat** | Status aktif kiosk **ditambah `Confirmed`** — task backend kecil, tanpa migration | Pasien dengan pesanan *Dikonfirmasi* tetap diarahkan ke pendaftaran baru (kunjungan ganda) |
| 7 | Bacaan Billing | **`INT-09` sementara** sesuai `LAB-DEC-220`, dihapus saat `LAB-REQ-008` dijawab | — |

## 4. Cara menjawab

Cukup *"setuju ketujuh butir"*, atau sebutkan nomor butir yang ingin diubah.

## 5. Sesudah disetujui — ✅ diterapkan 2026-10-08

| Artefak | Perubahan |
|---|---|
| `LAB-API-v1` | `r42` `approved` — bagian 37; 37.3 butir 2 kini menyebut `Confirmed` sebagai as-is |
| `LAB-PERM-v1` | revision 15 `approved` — bagian 17 |
| `LAB-STATE-v1` | `r9` `approved` — bagian 11 |
| `LAB-VAL-v1` | `r18` `approved` — bagian 20 (`VAL-152`, `VAL-153`) |
| `LAB-INT-v1` | `r6` `approved` — bagian 10 (`INT-09` tetap **sementara** sampai `LAB-REQ-008` dijawab) |
| `02-backend-architecture.md` | Bagian 26: status kontrak `approved`; cacat 26.6 diperbaiki |
| `04-prd-to-mvp.md`, `testing/acceptance-test-matrix.md` | Status kontrak `approved`; DoD bagian 27 diperbarui |
| Kode backend (butir 6) | `LabOrderStatus.Confirmed` ditambahkan ke status aktif `GetKioskPendingByPatientAsync`; build lolos, uji baca devYoga: `LAB-RSMMC-000013` dan `000006` *Dikonfirmasi* kini tampil — **belum di-commit**, pemilik modul yang meng-commit |
| `blueprint-manifest.md` | rev 103 |
